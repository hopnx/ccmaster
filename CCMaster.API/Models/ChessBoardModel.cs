using CCMaster.API.Services;
using CoreLibrary.Base;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Timers;

namespace CCMaster.API.Models
{
    static public class Color
    {
        public const string RED = "red";
        public const string BLACK = "black";
        static public string Opposite(string color)
        {
            if (color == Color.RED)
                return Color.BLACK;
            else
                return Color.RED;
        }
        static public string GetName(string color)
        {
            switch (color)
            {
                case RED: return "đỏ";
                case BLACK: return "đen";
                default: return "unknown";
            }
        }
    }
    public class BoardStatus
    {
        public const string LOCKED = "Locked";
        public const string NEW = "New";
        public const string READY = "Ready";
        public const string START_GAME = "StartGame";
        public const string PLAYING = "Playing";
        public const string GAMEOVER = "GameOver";
        public const string SIMULATOR = "Simulator";
    }
    public class Direction
    {
        public const string LEFT = "Left";
        public const string RIGHT = "Right";
        public const string TOP = "Top";
        public const string BOTTOM = "Bottom";
    }
    public struct Position
    {
        public Position(int row, int col)
        {
            Row = row;
            Col = col;
        }
        public int Row { get; set; }
        public int Col { get; set; }
        public Position Inverse() { return new Position(Board.ROWS - Row + 1, Board.COLS - Col + 1); }
    }
    static public class ItemType
    {
        public const string KING = "king";
        public const string ADVISOR = "advisor";
        public const string ELEPHANT = "elephant";
        public const string CHARIOT = "chariot";
        public const string CANON = "canon";
        public const string HORSE = "horse";
        public const string PAWN = "pawn";

        static public string GetName(string type)
        {
            switch (type)
            {
                case KING: return "Tướng";
                case ADVISOR: return "Sĩ";
                case ELEPHANT: return "Tượng";
                case CHARIOT: return "Xe";
                case HORSE: return "Mã";
                case CANON: return "Pháo";
                case PAWN: return "Tốt";
                default:
                    return "unknown";
            }
        }
    }
    public class Item
    {
        private Board _board;
        public string Type { get; set; }
        public string Color { get; set; }
        public Position Position { get; set; }
        public bool IsAlive { get { return Position.Row > 0 && Position.Col > 0; } }
        public List<Position> Scopes { get; private set; }
        public List<Position> CheckMateScopes { get; private set; }
        private List<Item> attackers { get; set; }
        private List<Item> attackTargets { get; set; }
        private List<Item> protectors { get; set; }
        private List<Item> protectTargets { get; set; }
        public int Row { get { return Position.Row; } }
        public int Col { get { return Position.Col; } }
        public int InverseRow { get { if (Position.Row <= 0) return 0; else return Board.ROWS - Position.Row + 1; } }
        public int InverseCol { get { if (Position.Col <= 0) return 0; else return Board.COLS - Position.Col + 1; } }
        public Position InversePosition { get { return new Position(Board.ROWS - Row + 1, Board.COLS - Col + 1); } }
        public void Reset()
        {
            SetPosition(0, 0);
            ClearTacticalInfo();
        }

        public Item(Board board, string color, string type)
        {
            _board = board;
            Type = type;
            Color = color;
            Reset();
        }
        public void SetPosition(Position position)
        {
            Position = position;
        }
        public void SetPosition(int row, int col)
        {
            Position = new Position { Row = row, Col = col };
        }
        public void Killed()
        {
            SetPosition(0, 0);
            ClearTacticalInfo();
        }
        public void FocusTarget(Item target)
        {
            if (target == null)
                return;
            if (target.Color == Color)
            {
                Protect(target);
            }
            else
            if (!attackTargets.Contains(target))
            {
                Attack(target);
            }
        }
        public void Protect(Item target)
        {
            if (target == null || target.Color != Color)
                return;
            if (!protectTargets.Contains(target))
            {
                protectTargets.Add(target);
                target.ProtectedBy(this);
            }
        }
        public void ProtectedBy(Item protector)
        {
            if (protector == null || this.Color != protector.Color)
                return;
            if (!protectors.Contains(protector))
            {
                protectors.Add(protector);
            }
        }
        public bool IsProtectedBy(Item item)
        {
            return protectors.Contains(item);
        }
        public void Attack(Item target)
        {
            if (target == null || target.Color == Color)
                return;
            if (!attackTargets.Contains(target))
            {
                attackTargets.Add(target);
                target.AttackedBy(this);
            }
        }
        public void AttackedBy(Item attacker)
        {
            if (attacker == null || this.Color == attacker.Color)
                return;
            if (!attackers.Contains(attacker))
            {
                attackers.Add(attacker);
            }
        }
        public bool IsAttackedBy(Item item)
        {
            return attackers.Contains(item);
        }
        public bool IsAttacked()
        {
            return (attackers.Count > 0);
        }
        public List<Item> AttackTargets { get { return attackTargets; } }
        public bool IsProtected()
        {
            return (protectors.Count > 0);
        }
        public void RemoveAttackTarget(Item target)
        {
            if (attackTargets.Contains(target))
            {
                attackTargets.Remove(target);
                target.RemoveAttacker(this);
            }
        }
        public void RemoveAttacker(Item attacker)
        {
            if (attackers.Contains(attacker))
            {
                attackers.Remove(attacker);
                attacker.RemoveAttackTarget(this);
            }
        }
        public void RemoveProtectTarget(Item target)
        {
            if (protectTargets.Contains(target))
            {
                protectTargets.Remove(target);
                target.RemoveProtector(this);
            }
        }
        public void RemoveProtector(Item protector)
        {
            if (protectors.Contains(protector))
            {
                protectors.Remove(protector);
                protector.RemoveProtectTarget(this);
            }
        }
        public void ClearTacticalInfo()
        {
            if (Scopes == null)
                Scopes = new List<Position>();
            else
                Scopes.Clear();

            if (attackers == null)
                attackers = new List<Item>();
            else
            {
                for (int i = attackers.Count - 1; i >= 0; i--)
                {
                    RemoveAttacker(attackers[i]);
                }
                attackers.Clear();
            }
            if (attackTargets == null)
                attackTargets = new List<Item>();
            else
            {
                for (int i = attackTargets.Count - 1; i >= 0; i--)
                {
                    RemoveAttackTarget(attackTargets[i]);
                }
                attackTargets.Clear();
                attackTargets.Clear();
            }

            if (protectors == null)
                protectors = new List<Item>();
            else
            {
                for (int i = protectors.Count - 1; i >= 0; i--)
                {
                    RemoveProtector(protectors[i]);
                }
                protectors.Clear();
            }
            if (protectTargets == null)
                protectTargets = new List<Item>();
            else
            {
                for (int i = protectTargets.Count - 1; i >= 0; i--)
                {
                    RemoveProtectTarget(protectTargets[i]);
                }
                protectTargets.Clear();
            }
        }
        public void SetScopes(List<Position> scopes)
        {
            Scopes = scopes;
        }
        public HashSet<Position> GetScopeFromAround()
        {
            HashSet<Position> result = new HashSet<Position>();
            switch (this.Type)
            {
                case ItemType.ELEPHANT:
                    if (this.Row == 1)
                    {
                        if (this.Col == 3 || this.Col == 7)
                        {
                            result.Add(new Position(this.Row + 1, this.Col - 1));
                            result.Add(new Position(this.Row + 1, this.Col + 1));
                        }
                    }
                    else
                    if (this.Row == 3)
                    {
                        if (this.Col == 1)
                        {
                            result.Add(new Position(this.Row + 1, this.Col + 1));
                            result.Add(new Position(this.Row - 1, this.Col + 1));
                        }
                        else
                        if (this.Col == 5)
                        {
                            result.Add(new Position(this.Row + 1, this.Col + 1));
                            result.Add(new Position(this.Row - 1, this.Col + 1));
                            result.Add(new Position(this.Row + 1, this.Col - 1));
                            result.Add(new Position(this.Row - 1, this.Col - 1));
                        }
                        else
                        if (this.Col == 9)
                        {
                            result.Add(new Position(this.Row + 1, this.Col - 1));
                            result.Add(new Position(this.Row - 1, this.Col - 1));
                        }
                    }
                    else
                    if (this.Row == 5)
                    {
                        if (this.Col == 3 || this.Col == 7)
                        {
                            result.Add(new Position(this.Row - 1, this.Col - 1));
                            result.Add(new Position(this.Row - 1, this.Col + 1));
                        }
                    }
                    break;
                case ItemType.HORSE:
                    if (this.Row > 1)
                    {
                        result.Add(new Position(this.Row - 1, this.Col));
                    }
                    if (this.Row <= Board.ROWS - 1)
                    {
                        result.Add(new Position(this.Row + 1, this.Col));
                    }
                    if (this.Col > 1)
                    {
                        result.Add(new Position(this.Row, this.Col - 1));
                    }
                    if (this.Col <= Board.COLS - 1)
                    {
                        result.Add(new Position(this.Row, this.Col + 1));
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        public List<Position> GetScopeFromDefinition()
        {
            List<Position> scopes = new List<Position>();
            if (!IsAlive)
                return scopes;
            switch (this.Type)
            {
                case ItemType.KING:
                    if (this.Row > 1)
                        scopes.Add(new Position(this.Row - 1, this.Col));
                    if (this.Row < 3)
                        scopes.Add(new Position(this.Row + 1, this.Col));
                    if (this.Col > 4)
                        scopes.Add(new Position(this.Row, this.Col - 1));
                    if (this.Col < 6)
                        scopes.Add(new Position(this.Row, this.Col + 1));
                    break;
                case ItemType.ADVISOR:
                    if (this.Row == 1)
                    {
                        if (this.Col == 4)
                            scopes.Add(new Position(this.Row + 1, this.Col + 1));
                        else
                        if (this.Col == 6)
                            scopes.Add(new Position(this.Row + 1, this.Col - 1));
                    }
                    else
                    if (this.Row == 2)
                    {
                        if (this.Col == 5)
                        {
                            scopes.Add(new Position(this.Row - 1, this.Col - 1));
                            scopes.Add(new Position(this.Row - 1, this.Col + 1));
                            scopes.Add(new Position(this.Row + 1, this.Col + 1));
                            scopes.Add(new Position(this.Row + 1, this.Col - 1));
                        }
                    }
                    else
                    if (this.Row == 3)
                    {
                        if (this.Col == 4)
                            scopes.Add(new Position(this.Row - 1, this.Col + 1));
                        else
                      if (this.Col == 6)
                            scopes.Add(new Position(this.Row - 1, this.Col - 1));
                    }
                    break;
                case ItemType.ELEPHANT:
                    if (this.Row == 1)
                    {
                        if (this.Col == 3 || this.Col == 7)
                        {
                            scopes.Add(new Position(this.Row + 2, this.Col - 2));
                            scopes.Add(new Position(this.Row + 2, this.Col + 2));
                        }
                    }
                    else
                    if (this.Row == 3)
                    {
                        if (this.Col == 1)
                        {
                            scopes.Add(new Position(this.Row + 2, this.Col + 2));
                            scopes.Add(new Position(this.Row - 2, this.Col + 2));
                        }
                        else
                        if (this.Col == 5)
                        {
                            scopes.Add(new Position(this.Row + 2, this.Col + 2));
                            scopes.Add(new Position(this.Row - 2, this.Col + 2));
                            scopes.Add(new Position(this.Row + 2, this.Col - 2));
                            scopes.Add(new Position(this.Row - 2, this.Col - 2));
                        }
                        else
                        if (this.Col == 9)
                        {
                            scopes.Add(new Position(this.Row + 2, this.Col - 2));
                            scopes.Add(new Position(this.Row - 2, this.Col - 2));
                        }
                    }
                    else
                    if (this.Row == 5)
                    {
                        if (this.Col == 3 || this.Col == 7)
                        {
                            scopes.Add(new Position(this.Row - 2, this.Col - 2));
                            scopes.Add(new Position(this.Row - 2, this.Col + 2));
                        }
                    }
                    break;
                case ItemType.HORSE:
                    if (this.Row > 1)
                    {
                        if (this.Col > 2)
                            scopes.Add(new Position(this.Row - 1, this.Col - 2));
                        if (this.Col <= Board.COLS - 2)
                            scopes.Add(new Position(this.Row - 1, this.Col + 2));
                    }
                    if (this.Row > 2)
                    {
                        if (this.Col > 1)
                            scopes.Add(new Position(this.Row - 2, this.Col - 1));
                        if (this.Col <= Board.COLS - 1)
                            scopes.Add(new Position(this.Row - 2, this.Col + 1));
                    }
                    if (this.Row <= Board.ROWS - 2)
                    {
                        if (this.Col > 1)
                            scopes.Add(new Position(this.Row + 2, this.Col - 1));
                        if (this.Col <= Board.COLS - 1)
                            scopes.Add(new Position(this.Row + 2, this.Col + 1));
                    }
                    if (this.Row <= Board.ROWS - 1)
                    {
                        if (this.Col > 2)
                            scopes.Add(new Position(this.Row + 1, this.Col - 2));
                        if (this.Col <= Board.COLS - 2)
                            scopes.Add(new Position(this.Row + 1, this.Col + 2));
                    }
                    break;
                case ItemType.CANON:
                case ItemType.CHARIOT:
                    for (int i = 1; i <= Board.ROWS; i++)
                    {
                        if (i != this.Row)
                            scopes.Add(new Position(i, this.Col));
                    }
                    for (int i = 1; i <= Board.COLS; i++)
                    {
                        if (i != this.Col)
                            scopes.Add(new Position(this.Row, i));
                    }
                    break;
                case ItemType.PAWN:
                    if (this.Row <= Board.ROWS - 1)
                        scopes.Add(new Position(this.Row + 1, this.Col));
                    if (this.Row >= 6)  // river passed
                    {
                        if (this.Col > 1)
                            scopes.Add(new Position(this.Row, this.Col - 1));
                        if (this.Col <= Board.COLS - 1)
                            scopes.Add(new Position(this.Row, this.Col + 1));
                    }
                    break;
                default:
                    break;
            }
            return scopes;
        }
        public void MoveTo(Position newPosition)
        {
            Position = newPosition;
        }
    }
    public class PlayMode
    {
        public const string MODE_PLAY_YOURSELF = "Play YourSelf";
        public const string MODE_PLAYER_VS_PLAYER = "Player vs Player";
        public const string MODE_PLAYER_VS_MACHINE = "Player vs Machine";
    }
    public class PlayerInBoard
    {
        public Guid Id { get; set; }
        public virtual Player Profile { get; set; }
        public bool ReadyToPlay { get; set; }
        public double TotalTime { get; set; }
        public double TotalMoveTime { get; set; }
        public double RemainTime { get; set; }
        public double RemainMoveTime { get; set; }
        public bool IsYourTurn { get; set; }
        public bool StartThinking { get; set; }
        public DateTime StartThinkingAt { get; set; }
    }
    public class GameConfig
    {
        public const int BONUS_SCORE = 10;
        public const int WARNING_ATTACK_COUNT = 3;
        public const int MAX_ATTACK_COUNT = 6;
        public const int WARNING_CHECKMATE_COUNT = 3;
        public const int MAX_CHECKMATE_COUNT = 6;

    }
    public class Board
    {
        public const int ROWS = 10;
        public const int COLS = 9;
        private readonly IBoardService _service;
        public Dictionary<Guid, string> ConnectionByPlayer { get; set; }
        public Guid Id { get; private set; }
        public string Mode { get; private set; }
        public virtual PlayerInBoard Owner { get; private set; }
        public virtual PlayerInBoard RedPlayer { get; private set; }
        public virtual PlayerInBoard BlackPlayer { get; private set; }
        public virtual PlayerInBoard WinPlayer { get; set; }
        public virtual PlayerInBoard LosePlayer { get; set; }
        public string Status { get; set; }
        public int TotalTime { get; set; }
        public int MoveTime { get; set; }
        public int BetCoin { get; set; }
        public Dictionary<Guid,PlayerInBoard> Observers { get; set; }
        public string Result { get; set; }
        public static string RED_WIN = "red_win";
        public static string BLACK_WIN = "black_win";
        public static string WIN = "win";
        public static string LOSE = "lose";
        public static string DRAW = "draw";
        public List<Item> Items { get; private set; }
        public HashSet<Position> redScopes { get; private set; }
        public HashSet<Position> blackScopes { get; private set; }
        private HashSet<Position> Scopes(string color)
        {
            if (color == Color.RED)
                return redScopes;
            else
                return blackScopes;
        }
        public string Turn { get; private set; }
        public Item RedKing { get; private set; }
        public Item BlackKing { get; private set; }
        public Timer RedTotalTimer { get; set; }
        public Timer RedMoveTimer { get; set; }
        public Timer BlackTotalTimer { get; set; }
        public Timer BlackMoveTimer { get; set; }
        public DateTime RedStartThinkingAt { get; set; }

        public Item King(string side)
        {
            if (side == Color.RED)
                return RedKing;
            else
                return BlackKing;
        }
        public bool IsCheckMated { get; private set; }
        public bool IsGameOver { get; private set; }

        private List<Movement> listRedMovement { get; set; }
        private List<Movement> listBlackMovement { get; set; }
        public class Movement
        {
            public Item Item { get; set; }
            public Position Departure { get; private set; }
            public Position Destination { get; private set; }
            public List<Item> AttackTargets { get; private set; }
            public int ConsecutiveAttackCount { get; private set; }
            public int ConsecutiveCheckMateCount { get; private set; }
            private Movement _previousMovement { get; set; }
            public Movement(Item item, Position destination, Movement previous)
            {
                Item = item;
                Departure = item.Position;
                Destination = destination;
                _previousMovement = previous;
                AttackTargets = new List<Item>();
            }
            public void Analyse()
            {
                Item.AttackTargets.ForEach(item =>
                {
                    AttackTargets.Add(item);
                });
                if (_previousMovement == null)
                {
                    if (AttackTargets.Count > 0)
                    {
                        for (int i = 0; i < AttackTargets.Count; i++)
                        {
                            Item target = AttackTargets[i];
                            if (!target.IsProtected())
                                ConsecutiveAttackCount = 1;
                            if (target.Type == ItemType.KING)
                                ConsecutiveCheckMateCount = 1;
                            if (ConsecutiveAttackCount == 1 && ConsecutiveCheckMateCount == 1)
                                break;
                        }
                    }
                    else
                    {
                        ConsecutiveAttackCount = 0;
                        ConsecutiveCheckMateCount = 0;
                    }
                }
                else
                {
                    if (AttackTargets.Count > 0)
                    {
                        ConsecutiveAttackCount = _previousMovement.ConsecutiveAttackCount;
                        ConsecutiveCheckMateCount = _previousMovement.ConsecutiveCheckMateCount;

                        for (int i = 0; i < AttackTargets.Count; i++)
                        {
                            Item target = AttackTargets[i];
                            if (!_previousMovement.AttackTargets.Contains(target))
                            {
                                ConsecutiveAttackCount = 0;
                                break;
                            }
                        }
                        bool increasedAttack = false, increasedCheckmate = false;
                        for (int i = 0; i < AttackTargets.Count; i++)
                        {
                            Item target = AttackTargets[i];
                            if (!target.IsProtected())
                            {
                                ConsecutiveAttackCount += 1;
                                increasedAttack = true;
                            }
                            if (target.Type == ItemType.KING)
                            {
                                ConsecutiveCheckMateCount += 1;
                                increasedCheckmate = true;
                            }
                            if (increasedAttack && increasedCheckmate)
                                break;
                        }
                    }
                    else
                    {
                        ConsecutiveAttackCount = 0;
                        ConsecutiveCheckMateCount = 0;
                    }
                }
            }
        }

        public Board(Board board)
        {
            Status = BoardStatus.SIMULATOR;
            Turn = board.Turn;
            // clone item list
            Items = new List<Item>();
            board.Items.Where(item => item.IsAlive).ToList().ForEach(item =>
            {
                Item cloneItem = new Item(this, item.Color, item.Type);
                cloneItem.SetPosition(item.Position);
                Items.Add(cloneItem);
            });
            SetKings();
            ClearTacticalInfo();
        }
        public void ClearTacticalInfo()
        {
            redScopes = new HashSet<Position>();
            blackScopes = new HashSet<Position>();
            listRedMovement = new List<Movement>();
            listBlackMovement = new List<Movement>();
        }
        public void SetKings()
        {
            RedKing = Items.Find(item => item.Color == Color.RED && item.Type == ItemType.KING);
            BlackKing = Items.Find(item => item.Color == Color.BLACK && item.Type == ItemType.KING);

        }
        public Board(Player owner, IBoardService service)
        {
            _service = service;
            Id = Guid.NewGuid();
            Mode = PlayMode.MODE_PLAYER_VS_PLAYER;
            Status = BoardStatus.NEW;
            Owner = owner.MapToPlayerInBoard();
            TotalTime = BoardSetting.DefaultTotalTime;
            MoveTime = BoardSetting.DefaultMoveTime;
            BetCoin = BoardSetting.DefaultBetCoin;
            ConnectionByPlayer = new Dictionary<Guid, string>();
            Observers = new Dictionary<Guid, PlayerInBoard>();

            AssignPlayer(owner);
            InitItems();
            ClearTacticalInfo();
        }
        public bool AssignPlayer(Player player)
        {
            string[] status = new string[] { BoardStatus.START_GAME, BoardStatus.PLAYING };
            if (status.Contains(Status))
            {
                throw new Exception("Không được thay đổi người chơi tại bàn đang chơi");
            }

            if ((RedPlayer != null && RedPlayer.Id == player.Id) ||
                (BlackPlayer != null && BlackPlayer.Id == player.Id))
                return true;
            if (RedPlayer == null)
            {
                RedPlayer = player.MapToPlayerInBoard();
                RedPlayer.TotalTime = TotalTime;
                RedPlayer.TotalMoveTime = MoveTime;
                RedPlayer.RemainTime = TotalTime;
                RedPlayer.RemainMoveTime = MoveTime;
                RedPlayer.StartThinking = false;
                return true;
            }
            else
            if (BlackPlayer == null)
            {
                BlackPlayer = player.MapToPlayerInBoard();
                BlackPlayer.TotalTime = TotalTime;
                BlackPlayer.TotalMoveTime = MoveTime;
                BlackPlayer.RemainTime = TotalTime;
                BlackPlayer.RemainMoveTime = MoveTime;
                BlackPlayer.StartThinking = false;
                return true;
            }
            else
                return false;
        }
        public void SaveConnection(Guid playerId, string connectionId)
        {
            if (!ConnectionByPlayer.ContainsKey(playerId))
                ConnectionByPlayer.Add(playerId, connectionId);
        }
        public void RemoveConnection(Guid playerId)
        {
            ConnectionByPlayer.Remove(playerId);
        }
        public void SwitchTurn()
        {
            Turn = Color.Opposite(Turn);
            RedPlayer.IsYourTurn = (Turn == Color.RED);
            BlackPlayer.IsYourTurn = (Turn == Color.BLACK);

            DateTime now = DateTime.Now;
            if (RedPlayer.IsYourTurn && !RedPlayer.StartThinking)
            {
                //start thinking
                RedPlayer.StartThinking = true;
                RedPlayer.StartThinkingAt = now;
                RedTotalTimer.Interval = RedPlayer.RemainTime * 1000;
                RedTotalTimer.Start();
                RedPlayer.RemainMoveTime = RedPlayer.TotalMoveTime;
                RedMoveTimer.Interval = RedPlayer.RemainMoveTime * 1000;
                RedMoveTimer.Start();
            }
            else
            if (RedPlayer.StartThinking && !RedPlayer.IsYourTurn)
            {
                //stop thingking
                UpdateRemainTime(RedPlayer, now);
                RedPlayer.StartThinking = false;
                RedTotalTimer.Stop();
                RedMoveTimer.Stop();
            }

            if (BlackPlayer.IsYourTurn && !BlackPlayer.StartThinking)
            {
                //start thinking
                BlackPlayer.StartThinking = true;
                BlackPlayer.StartThinkingAt = now;
                BlackTotalTimer.Interval = BlackPlayer.RemainTime * 1000;
                BlackTotalTimer.Start();
                BlackPlayer.RemainMoveTime = BlackPlayer.TotalMoveTime;
                BlackMoveTimer.Interval = BlackPlayer.RemainMoveTime * 1000;
                BlackMoveTimer.Start();
            }
            else
            if (BlackPlayer.StartThinking && !BlackPlayer.IsYourTurn)
            {
                //stop thingking
                UpdateRemainTime(BlackPlayer, now);
                BlackPlayer.StartThinking = false;
                BlackTotalTimer.Stop();
                BlackMoveTimer.Stop();
            }
        }

        public void UpdateRemainTime(PlayerInBoard player, DateTime current)
        {
            if (player != null && player.StartThinking && player.StartThinkingAt.Year > 1)
            {
                TimeSpan elapsed = (current - player.StartThinkingAt);
                player.RemainTime -= Math.Round(elapsed.TotalSeconds);
                player.RemainMoveTime -= Math.Round(elapsed.TotalSeconds);
            }
        }
        private void InitItems()
        {
            Items = new List<Item>();
            Items.Add(new Item(this, Color.RED, ItemType.KING));
            Items.Add(new Item(this, Color.BLACK, ItemType.KING));
            for (int i = 1; i <= 2; i++)
            {
                Items.Add(new Item(this, Color.RED, ItemType.ADVISOR));
                Items.Add(new Item(this, Color.RED, ItemType.ELEPHANT));
                Items.Add(new Item(this, Color.RED, ItemType.CHARIOT));
                Items.Add(new Item(this, Color.RED, ItemType.CANON));
                Items.Add(new Item(this, Color.RED, ItemType.HORSE));

                Items.Add(new Item(this, Color.BLACK, ItemType.ADVISOR));
                Items.Add(new Item(this, Color.BLACK, ItemType.ELEPHANT));
                Items.Add(new Item(this, Color.BLACK, ItemType.CHARIOT));
                Items.Add(new Item(this, Color.BLACK, ItemType.CANON));
                Items.Add(new Item(this, Color.BLACK, ItemType.HORSE));
            }
            for (int i = 1; i <= 5; i++)
            {
                Items.Add(new Item(this, Color.RED, ItemType.PAWN));
                Items.Add(new Item(this, Color.BLACK, ItemType.PAWN));
            }

            SetKings();
        }
        public void Reset()
        {
            IsGameOver = false;
            BuildCaseDefault();
            //BoardCase.DonXeThang(this); ;
            Turn = Color.RED;
            ClearTacticalInfo();
            AnalyseBoard(Turn);

            if (RedPlayer != null)
            {
                if (RedTotalTimer != null)
                    RedTotalTimer.Stop();
                if (RedMoveTimer != null)
                    RedMoveTimer.Stop();

                RedTotalTimer = CreateTimer(TotalTime, OnRedElapsedTime);
                RedMoveTimer = CreateTimer(MoveTime, OnRedElapsedTime);

                RedPlayer.TotalTime = TotalTime;
                RedPlayer.TotalMoveTime = MoveTime;
                RedPlayer.RemainTime = TotalTime;
                RedPlayer.RemainMoveTime = MoveTime;
                RedPlayer.StartThinking = false;
            }
            if (BlackPlayer != null)
            {
                if (BlackTotalTimer != null)
                    BlackTotalTimer.Stop();
                if (BlackMoveTimer != null)
                    BlackMoveTimer.Stop();

                BlackTotalTimer = CreateTimer(TotalTime, OnBlackElapsedTime);
                BlackMoveTimer = CreateTimer(MoveTime, OnBlackElapsedTime);

                BlackPlayer.TotalTime = TotalTime;
                BlackPlayer.TotalMoveTime = MoveTime;
                BlackPlayer.RemainTime = TotalTime;
                BlackPlayer.RemainMoveTime = MoveTime;
                BlackPlayer.StartThinking = false;
            }
        }
        public void ClearBoard()
        {
            Items.ForEach(item =>
            {
                item.Reset();
            });
            blackScopes.Clear();
            redScopes.Clear();
        }
        public void SetItem(string color, string type, int row, int col)
        {
            Item item = Items.Find(item => item.Color == color && item.Type == type && !item.IsAlive);
            if (item != null)
            {
                item.SetPosition(row, col);
            }
        }
        private List<Item> GetItems(string direction, Item root)
        {
            List<Item> result = new List<Item>();
            switch (direction)
            {
                case Direction.RIGHT:
                    result.AddRange(
                        Items.FindAll(item =>
                            (item.Color == root.Color && item.Row == root.Row && item.Col < root.Col) ||
                            (item.Color != root.Color && item.InverseRow == root.Row && item.InverseCol < root.Col))
                             .OrderByDescending(item =>
                             {
                                 if (item.Color == root.Color) return item.Col; else return item.InverseCol;
                             }));
                    break;
                case Direction.LEFT:
                    result.AddRange(
                        Items.FindAll(item =>
                            (item.Color == root.Color && item.Row == root.Row && item.Col > root.Col) ||
                            (item.Color != root.Color && item.InverseRow == root.Row && item.InverseCol > root.Col))
                             .OrderBy(item =>
                             {
                                 if (item.Color == root.Color) return item.Col; else return item.InverseCol;
                             }));
                    break;
                case Direction.BOTTOM:
                    result.AddRange(
                        Items.FindAll(item =>
                            (item.Color == root.Color && item.Col == root.Col && item.Row < root.Row) ||
                            (item.Color != root.Color && item.InverseCol == root.Col && item.InverseRow < root.Row))
                             .OrderByDescending(item =>
                             {
                                 if (item.Color == root.Color) return item.Row; else return item.InverseRow;
                             }));
                    break;
                case Direction.TOP:
                    result.AddRange(
                        Items.FindAll(item =>
                            (item.Color == root.Color && item.Col == root.Col && item.Row > root.Row) ||
                            (item.Color != root.Color && item.InverseCol == root.Col && item.InverseRow > root.Row))
                             .OrderBy(item =>
                             {
                                 if (item.Color == root.Color) return item.Row; else return item.InverseRow;
                             }));
                    break;
                default:
                    break;
            }
            return result;
        }

        public Item PickItem(string color, string type, int row, int col)
        {
            return Items.Find(item => item.Color == color && item.Type == item.Type && item.Row == row && item.Col == col);
        }
        public Item FindItem(string side, int row, int col)
        {
            Item item = Items.Find(item => item.Color == side && item.Row == row && item.Col == col);
            if (item != null)
                return item;
            else
                return Items.Find(item => item.Color == Color.Opposite(side) && item.InverseRow == row && (item.InverseCol == col));
        }
        public Timer CreateTimer(int seconds, ElapsedEventHandler handler)
        {
            Timer result = new Timer();
            result.Interval = 1000 * seconds;
            result.Elapsed += handler;
            result.AutoReset = false;
            return result;
        }
        public void OnRedElapsedTime(Object source, System.Timers.ElapsedEventArgs e)
        {
            _service.BlackWin(this, "Bên ĐỎ hết thời gian suy nghĩ");
        }
        public void OnBlackElapsedTime(Object source, System.Timers.ElapsedEventArgs e)
        {
            _service.RedWin(this, "Bên ĐEN hết thời gian suy nghĩ");
        }

        public string CheckMoveItemTo(Item item, Position destination)
        {
            return "";
        }
        public void BuildCaseDefault()
        {
            ClearBoard();
            SetItem(Color.RED, ItemType.CHARIOT, 1, 1);
            SetItem(Color.RED, ItemType.HORSE, 1, 2);
            SetItem(Color.RED, ItemType.ELEPHANT, 1, 3);
            SetItem(Color.RED, ItemType.ADVISOR, 1, 4);
            SetItem(Color.RED, ItemType.KING, 1, 5);
            SetItem(Color.RED, ItemType.ADVISOR, 1, 6);
            SetItem(Color.RED, ItemType.ELEPHANT, 1, 7);
            SetItem(Color.RED, ItemType.HORSE, 1, 8);
            SetItem(Color.RED, ItemType.CHARIOT, 1, 9);

            SetItem(Color.RED, ItemType.CANON, 3, 2);
            SetItem(Color.RED, ItemType.CANON, 3, 8);

            SetItem(Color.RED, ItemType.PAWN, 4, 1);
            SetItem(Color.RED, ItemType.PAWN, 4, 3);
            SetItem(Color.RED, ItemType.PAWN, 4, 5);
            SetItem(Color.RED, ItemType.PAWN, 4, 7);
            SetItem(Color.RED, ItemType.PAWN, 4, 9);

            SetItem(Color.BLACK, ItemType.CHARIOT, 1, 1);
            SetItem(Color.BLACK, ItemType.HORSE, 1, 2);
            SetItem(Color.BLACK, ItemType.ELEPHANT, 1, 3);
            SetItem(Color.BLACK, ItemType.ADVISOR, 1, 4);
            SetItem(Color.BLACK, ItemType.KING, 1, 5);
            SetItem(Color.BLACK, ItemType.ADVISOR, 1, 6);
            SetItem(Color.BLACK, ItemType.ELEPHANT, 1, 7);
            SetItem(Color.BLACK, ItemType.HORSE, 1, 8);
            SetItem(Color.BLACK, ItemType.CHARIOT, 1, 9);

            SetItem(Color.BLACK, ItemType.CANON, 3, 2);
            SetItem(Color.BLACK, ItemType.CANON, 3, 8);

            SetItem(Color.BLACK, ItemType.PAWN, 4, 1);
            SetItem(Color.BLACK, ItemType.PAWN, 4, 3);
            SetItem(Color.BLACK, ItemType.PAWN, 4, 5);
            SetItem(Color.BLACK, ItemType.PAWN, 4, 7);
            SetItem(Color.BLACK, ItemType.PAWN, 4, 9);
        }
        public void SwitchSide()
        {
            PlayerInBoard temp = RedPlayer;
            RedPlayer = BlackPlayer;
            BlackPlayer = temp;
        }
        public void SetPlayer(string color, PlayerInBoard player)
        {
            if (color == Color.RED)
                RedPlayer = player;
            else
            if (color == Color.BLACK)
                BlackPlayer = player;
            if ((RedPlayer == null || BlackPlayer == null) && (Status == BoardStatus.READY))
                Status = BoardStatus.NEW;
        }
        public void MoveItemTo(Item item, Position position)
        {
            if (Status != BoardStatus.SIMULATOR)
                AddMovement(item, position);
            Item targetItem = FindItem(item.Color, position.Row, position.Col);
            if (targetItem != null)
            {
                targetItem.Killed();
            }
            item.MoveTo(position);
            if (Status == BoardStatus.START_GAME)
                Status = BoardStatus.PLAYING;
        }
        public void AddMovement(Item item, Position position)
        {
            List<Movement> list = GetLastMovement(item.Color);
            Movement lastMovement = list.LastOrDefault();
            if (lastMovement != null && lastMovement.Item != item)
            {
                list.Clear();
                lastMovement = null;
            }
            Movement newMovement = new Movement(item, position, lastMovement);
            list.Add(newMovement);
        }
        public bool IsCheckmated(string side)
        {
            AnalyseBoardWithTheBasicRules(Color.Opposite(side));
            Item myKing = GetKing(side);
            return myKing.IsAttacked();
        }
        private Item GetKing(string side)
        {
            return Items.Find(item => item.Color == side && item.Type == ItemType.KING);
        }
        private List<Item> GetItemList(string side)
        {
            return Items.FindAll(item => item.Color == side && item.IsAlive);
        }
        private void UpdateScope(string side)
        {
            HashSet<Position> scopes = new HashSet<Position>();
            List<Item> items = GetItemList(side);
            items.ForEach(item =>
                item.Scopes.ForEach(scope => scopes.Add(scope)));
            if (side == Color.RED)
                redScopes = scopes;
            else
                blackScopes = scopes;
        }
        public void AnalyseBoard()
        {
            if (CheckConsecutiveMovement(Turn))
                return;
            SwitchTurn();
            AnalyseBoard(Turn);
        }
        public void AnalyseBoard(string side)
        {
            AnalyseBoardWithTheBasicRules(side);
            AnalyseBoardWithTheCheckMateRules(side);
            if (CheckGameOver(side))
                return;
        }
        public bool CheckGameOver(string side)
        {
            IsCheckMated = IsCheckmated(side);
            IsGameOver = (Scopes(side).Count == 0);
            if (IsGameOver)
            {
                if (IsCheckMated)
                    WarningMessage = String.Format("Bên {0} bị chiếu hết", (side == Color.RED) ? "ĐỎ" : "ĐEN");
                else
                    WarningMessage = String.Format("Bên {0} hết nước đi", (side == Color.RED) ? "ĐỎ" : "ĐEN");
            }
            return IsGameOver;
        }
        private void AnalyseBoardWithTheBasicRules(string side)
        {
            List<Item> items = GetItemList(side);
            items.ForEach(item => item.ClearTacticalInfo());
            items.ForEach(item =>
            {
                AnalyseItemFollowingTheBasicRules(item);
            });
            UpdateScope(side);
        }
        private void AnalyseItemFollowingTheBasicRules(Item item)
        {
            List<Position> scopes = item.GetScopeFromDefinition();
            switch (item.Type)
            {
                case ItemType.KING:
                    Item enemyKing = GetKing(Color.Opposite(item.Color));
                    if (enemyKing.InverseCol == item.Col)
                    {
                        Item betweenItem = Items.Find(i =>
                            i.Color == item.Color && i.Col == item.Col && i.Row > item.Row && i.Row < enemyKing.InverseRow
                            ||
                            i.Color != item.Color && i.InverseCol == item.Col && i.InverseRow > item.Row && i.InverseRow < enemyKing.InverseRow
                        );
                        if (betweenItem == null)
                            item.Attack(enemyKing);
                    }
                    break;
                case ItemType.ELEPHANT:
                    HashSet<Position> elephaneAroundScopes = item.GetScopeFromAround();
                    List<Item> elephanAroundItems = Items.FindAll(i =>
                        i.Color == item.Color && elephaneAroundScopes.Contains(i.Position) ||
                        i.Color != item.Color && elephaneAroundScopes.Contains(i.InversePosition)
                        );
                    elephanAroundItems.ForEach(i =>
                    {
                        if (i.Color == item.Color)
                            scopes.RemoveAll(position =>
                             position.Row == (2 * i.Row - item.Row) &&
                             position.Col == (2 * i.Col - item.Col)
                             );
                        else
                            scopes.RemoveAll(position =>
                             position.Row == (2 * i.InverseRow - item.Row) &&
                             position.Col == (2 * i.InverseCol - item.Col));

                    });
                    break;
                case ItemType.HORSE:
                    HashSet<Position> horseAroundScopes = item.GetScopeFromAround();
                    List<Item> horesAroundItems = Items.FindAll(i =>
                        i.Color == item.Color && horseAroundScopes.Contains(i.Position) ||
                        i.Color != item.Color && horseAroundScopes.Contains(i.InversePosition)
                        );
                    horesAroundItems.ForEach(i =>
                    {
                        if (i.Color == item.Color)
                            scopes.RemoveAll(scope => scope.Col == (2 * i.Col - item.Col) || scope.Row == (2 * i.Row - item.Row));
                        else
                            scopes.RemoveAll(scope => scope.Col == (2 * i.InverseCol - item.Col) || scope.Row == (2 * i.InverseRow - item.Row));
                    });
                    break;
                case ItemType.CHARIOT:
                    //right scan
                    Item chariotNearItem = GetItems(Direction.RIGHT, item).FirstOrDefault();
                    if (chariotNearItem != null)
                    {
                        if (chariotNearItem.Color == item.Color)
                            scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col < chariotNearItem.Col);
                        else
                            scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col < chariotNearItem.InverseCol);
                    }
                    //left scan
                    chariotNearItem = GetItems(Direction.LEFT, item).FirstOrDefault();
                    if (chariotNearItem != null)
                    {
                        if (chariotNearItem.Color == item.Color)
                            scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col > chariotNearItem.Col);
                        else
                            scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col > chariotNearItem.InverseCol);
                    }
                    //bottom scan
                    chariotNearItem = GetItems(Direction.BOTTOM, item).FirstOrDefault();
                    if (chariotNearItem != null)
                    {
                        if (chariotNearItem.Color == item.Color)
                            scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row < chariotNearItem.Row);
                        else
                            scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row < chariotNearItem.InverseRow);
                    }
                    //top scan
                    chariotNearItem = GetItems(Direction.TOP, item).FirstOrDefault();
                    if (chariotNearItem != null)
                    {
                        if (chariotNearItem.Color == item.Color)
                            scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row > chariotNearItem.Row);
                        else
                            scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row > chariotNearItem.InverseRow);
                    }
                    break;
                case ItemType.CANON:
                    string color = item.Color;
                    List<Item> canonItems = new List<Item>();
                    Item canonFirst = null;
                    Item canonSecond = null;
                    //right scan
                    canonItems = GetItems(Direction.RIGHT, item);
                    canonFirst = canonItems.FirstOrDefault();
                    if (canonFirst != null)
                    {
                        canonSecond = null;
                        if (canonItems.Count > 1)
                            canonSecond = canonItems[1];

                        if (canonFirst != null && canonSecond != null)
                            item.FocusTarget(canonSecond);

                        if (canonFirst.Color == item.Color)
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col <= canonFirst.Col);
                            else
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col <= canonFirst.Col && scope.Col != canonSecond.InverseCol);
                        }
                        else
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col <= canonFirst.InverseCol);
                            else
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col <= canonFirst.InverseCol && scope.Col != canonSecond.InverseCol);
                        }


                    }
                    //left scan
                    canonItems = GetItems(Direction.LEFT, item);
                    canonFirst = canonItems.FirstOrDefault();
                    if (canonFirst != null)
                    {
                        canonSecond = null;
                        if (canonItems.Count > 1)
                            canonSecond = canonItems[1];

                        if (canonFirst != null && canonSecond != null)
                            item.FocusTarget(canonSecond);

                        if (canonFirst.Color == item.Color)
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col >= canonFirst.Col);
                            else
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col >= canonFirst.Col && scope.Col != canonSecond.InverseCol);
                        }
                        else
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col >= canonFirst.InverseCol);
                            else
                                scopes.RemoveAll(scope => scope.Row == item.Row && scope.Col >= canonFirst.InverseCol && scope.Col != canonSecond.InverseCol);
                        }
                    }
                    //bottom scan
                    canonItems = GetItems(Direction.BOTTOM, item);
                    canonFirst = canonItems.FirstOrDefault();
                    if (canonFirst != null)
                    {
                        canonSecond = null;
                        if (canonItems.Count > 1)
                            canonSecond = canonItems[1];

                        if (canonFirst != null && canonSecond != null)
                            item.FocusTarget(canonSecond);

                        if (canonFirst.Color == item.Color)
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row <= canonFirst.Row);
                            else
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row <= canonFirst.Row && scope.Row != canonSecond.InverseRow);
                        }
                        else
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row <= canonFirst.InverseRow);
                            else
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row <= canonFirst.InverseRow && scope.Row != canonSecond.InverseRow);
                        }
                    }
                    //top scan
                    canonItems = GetItems(Direction.TOP, item);
                    canonFirst = canonItems.FirstOrDefault();
                    if (canonFirst != null)
                    {
                        canonSecond = null;
                        if (canonItems.Count > 1)
                            canonSecond = canonItems[1];

                        if (canonFirst != null && canonSecond != null)
                            item.FocusTarget(canonSecond);

                        if (canonFirst.Color == item.Color)
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row >= canonFirst.Row);
                            else
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row >= canonFirst.Row && scope.Row != canonSecond.InverseRow);
                        }
                        else
                        {
                            if (canonSecond == null || canonSecond.Color == item.Color)
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row >= canonFirst.InverseRow);
                            else
                                scopes.RemoveAll(scope => scope.Col == item.Col && scope.Row >= canonFirst.InverseRow && scope.Row != canonSecond.InverseRow);
                        }
                    }
                    break;
                default:
                    break;
            }

            List<Item> targets = Items.FindAll(i => i.Color == item.Color && scopes.Contains(i.Position) || i.Color != item.Color && scopes.Contains(i.InversePosition));
            targets.ForEach(i =>
            {
                switch (item.Type)
                {
                    case ItemType.CANON:
                        break;
                    default:
                        item.FocusTarget(i);
                        if (i.Color == item.Color)
                            scopes.Remove(i.Position);
                        break;
                }
            });

            item.SetScopes(scopes);
        }
        private void AnalyseBoardWithTheCheckMateRules(string side)
        {
            List<Item> items = GetItemList(side);
            items.ForEach(item =>
            {
                HashSet<Position> invalidScopes = new HashSet<Position>();
                item.Scopes.ForEach(position =>
                {
                    Board board = this.GetBoardAfterMovingItemTo(item, position);
                    if (board.IsCheckmated(side))
                        invalidScopes.Add(position);
                });
                item.Scopes.RemoveAll(position => invalidScopes.Contains(position));
            });
            UpdateScope(side);
        }
        private Board GetBoardAfterMovingItemTo(Item item, Position position)
        {
            Board cloneBoard = new Board(this);

            Item cloneItem = cloneBoard.FindItem(item.Color, item.Row, item.Col);
            cloneBoard.MoveItemTo(cloneItem, position);
            return cloneBoard;
        }

        private bool CheckConsecutiveMovement(string side)
        {
            AnalyseBoardWithTheBasicRules(side);
            WarningMessage = null;
            WarningSide = null;
            List<Movement> list = GetLastMovement(side);
            if (list.Count == 0)
                return false;
            Movement lastMovement = list.LastOrDefault();
            lastMovement.Analyse();
            if (lastMovement.ConsecutiveAttackCount >= GameConfig.WARNING_ATTACK_COUNT)
            {
                if (lastMovement.ConsecutiveAttackCount < GameConfig.MAX_ATTACK_COUNT)
                {
                    WarningSide = Turn;
                    WarningMessage = String.Format("Bạn đã đuổi bắt quân đối phương {0} lần. Bạn sẽ bị xử thua nếu tiếp tục lặp lại nước đuổi bắt này", lastMovement.ConsecutiveAttackCount);
                }
                else
                {
                    IsGameOver = true;
                    WarningSide = Turn;
                    WarningMessage = String.Format("Bên {0} bị xử thua do đuổi bắt quân đối phương liên tục {1} lần", (side == Color.RED) ? "ĐỎ" : "ĐEN", GameConfig.MAX_ATTACK_COUNT);
                    return true;
                }
            }
            if (lastMovement.ConsecutiveCheckMateCount > GameConfig.WARNING_CHECKMATE_COUNT)
            {
                if (lastMovement.ConsecutiveCheckMateCount < GameConfig.MAX_CHECKMATE_COUNT)
                {
                    WarningSide = Turn;
                    WarningMessage = String.Format("Bạn đã chiếu liên tục đối phương {0} lần. Bạn sẽ bị xử thua nếu tiếp tục lặp lại nước chiếu này", lastMovement.ConsecutiveCheckMateCount);
                }
                else
                {
                    IsGameOver = true;
                    WarningMessage = String.Format("Bên {0} bị xử thua do phạm luật chiếu liên tục đối phương {1} lần", (side == Color.RED) ? "ĐỎ" : "ĐEN", GameConfig.MAX_CHECKMATE_COUNT);
                    return true;
                }
            }
            return false;
        }
        public string WarningMessage { get; private set; }
        public string WarningSide { get; private set; }
        private List<Movement> GetLastMovement(string side)
        {
            if (side == Color.RED)
                return listRedMovement;
            else
                return listBlackMovement;
        }
        public bool IsPlayer(Guid playerId)
        {
            return (RedPlayer != null && RedPlayer.Id == playerId) ||
                   (BlackPlayer != null && BlackPlayer.Id == playerId);
        }
        public bool IsRedPlayer(Guid playerId)
        {
            return (RedPlayer != null && RedPlayer.Id == playerId);
        }
        public bool IsBlackPlayer(Guid playerId)
        {
            return (BlackPlayer != null && BlackPlayer.Id == playerId);
        }
        public void UnAssigned(Guid playerId)
        {
            if (IsRedPlayer(playerId))
            {
                RedPlayer = null;
                Status = BoardStatus.NEW;
            }
            else
            if (IsBlackPlayer(playerId))
            {
                BlackPlayer = null;
                Status = BoardStatus.NEW;
            }
            else
                Observers.Remove(playerId);
            RemoveConnection(playerId);
        }
    }
    public class BoardSetting
    {
        public int TotalTime { get; set; }
        public int MoveTime { get; set; }
        public int BetCoin { get; set; }
        static public int DefaultTotalTime = 10 * 60;
        static public int DefaultMoveTime = 60;
        static public int DefaultBetCoin = 1000;

    }

}
