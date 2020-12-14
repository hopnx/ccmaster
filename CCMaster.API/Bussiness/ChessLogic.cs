using CCMaster.API.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CCMaster.API.Bussiness
{
    public class ExceptionItemNotFound : Exception
    {
        public ExceptionItemNotFound(string message) : base(message) { }
    }
    public class ExceptionInvalidMove : Exception
    {
        public ExceptionInvalidMove(string message) : base(message) { }
    }
    public class ExceptionItemNotMatched : Exception
    {
        public ExceptionItemNotMatched(string message) : base(message) { }
    }
    public class ExceptionInvalidChessPoint : Exception
    {
        public ExceptionInvalidChessPoint(string message) : base(message) { }
    }
    public enum ITEM_TYPE
    {
        PAWN = 1,
        ADVISOR = 2,
        ELEPHANT = 3,
        HORSE = 4,
        CANON = 5,
        CHARIOT = 6,
        KING = 7
    }
    public enum BOARD_SIDE
    {
        BLACK = -1,
        RED = 1
    }
    public enum MOVE_TYPE
    {
        BACKWARD = -1,
        SIDEWARD = 0,
        FORWARD = 1
    }
    
    public struct ChessVector
    {
        public int Y { get; private set; }
        public int X { get; private set; }
        static public ChessVector MoveLeft(int step = 1)
        {
            if (step < 0)
                throw new ExceptionInvalidMove(String.Format("Invalid MoveLeft with Step = {0}",step));
            return new ChessVector(0, -step);
        }
        static public ChessVector MoveRight(int step = 1)
        {
            if (step < 0)
                throw new ExceptionInvalidMove(String.Format("Invalid MoveRight with Step = {0}", step));
            return new ChessVector(0, step);
        }
        static public ChessVector MoveForward(int step = 1)
        {
            if (step < 0)
                throw new ExceptionInvalidMove(String.Format("Invalid MoveForward with Step = {0}", step));
            return new ChessVector(step,0);
        }
        static public ChessVector MoveBackward(int step = 1)
        {
            if (step < 0)
                throw new ExceptionInvalidMove(String.Format("Invalid MoveBackward with Step = {0}", step));
            return new ChessVector(-step, 0);
        }

        public ChessVector(int y,int x)
        {
            Y = y;
            X = x;
        }
    }
    public struct ChessPoint
    {
        public int Row { get; private set; }
        public int Col { get; private set; }
        public ChessPoint(int r,int c)
        {
            Row = r;
            Col = c;
        }
        public bool CanMoveTo(ChessVector vector)
        {
            ChessPoint newPoint = new ChessPoint { Row = Row + vector.Y, Col = Col + vector.X };
            return newPoint.Row >= 1 &&
                   newPoint.Row <= ChessBoard.RowSize &&
                   newPoint.Col >= 1 &&
                   newPoint.Col <= ChessBoard.ColumnSize;
        }
        public bool CanMoveTo(int r, int c)
        {
            ChessPoint newPoint = new ChessPoint { Row = Row + r, Col = Col + c };
            return newPoint.Row >= 1 &&
                   newPoint.Row <= ChessBoard.RowSize &&
                   newPoint.Col >= 1 &&
                   newPoint.Col <= ChessBoard.ColumnSize;
        }
        public ChessPoint MoveTo(ChessVector vector)
        {
            if (CanMoveTo(vector))
                return new ChessPoint { Row = Row + vector.Y, Col = Col + vector.X };
            else
                throw new ExceptionInvalidChessPoint(String.Format("Không thể di chuyển đến tọa độ [r:{0},c:{1}", Row + vector.Y, Col + vector.X));
        }
        public ChessPoint MoveTo(int Y, int X)
        {
            if (CanMoveTo(Y, X))
                return new ChessPoint { Row = Row + Y, Col = Col + X };
            else
                throw new ExceptionInvalidChessPoint(String.Format("Không thể di chuyển đến tọa độ [r:{0},c:{1}", Row + Y, Col + X));
        }
    }
    static public class ChessRule
    {
        static public string GetSideName(BOARD_SIDE side)
        {
            switch (side)
            {
                case BOARD_SIDE.RED: return "Đỏ";
                default: return "Đen";
            }
        }
        static public string GetTypeName(ITEM_TYPE type)
        {
            switch (type)
            {
                case ITEM_TYPE.KING: return "Tướng";
                case ITEM_TYPE.ADVISOR: return "Sĩ";
                case ITEM_TYPE.ELEPHANT: return "Tượng";
                case ITEM_TYPE.CHARIOT: return "Xe";
                case ITEM_TYPE.CANON: return "Pháo";
                case ITEM_TYPE.HORSE: return "Mã";
                default: return "Tốt";
            }
        }
    }
    public class ChessMan
    {
        private ChessBoard _board;
        public BOARD_SIDE Side { get; }
        public ITEM_TYPE Type { get; }
        private ChessPoint _currentPoint, _resetPoint;
        public int Row { get { return _currentPoint.Row; } }
        public int Col { get { return _currentPoint.Col; } }
        public ChessMan(ChessBoard board, BOARD_SIDE side, ITEM_TYPE type,int defaultRow,int defaultCol)
        {
            Side = side;
            Type = type;
            _currentPoint = new ChessPoint(0,0);
            _resetPoint = new ChessPoint(defaultRow,defaultCol);
            _board = board;
        }
        public void Reset()
        {
            _currentPoint = _resetPoint;
            _board.Update(this);
        }
        public ChessMan(BOARD_SIDE side, ITEM_TYPE type, ChessPoint position)
        {
            Side = side;
            Type = type;
            _currentPoint = position;
        }
        public bool IsAlive()
        {
            return (
                (_currentPoint.Row > 0) 
                && (_currentPoint.Row < ChessBoard.RowSize) 
                && (_currentPoint.Col > 0) 
                && (_currentPoint.Col < ChessBoard.ColumnSize)
                );
        }
        public void SetPoint(ChessPoint point)
        {
            SetPosition(point.Row, point.Col);
        }
        public void SetPosition(int row, int col)
        {
            if (ValidatePosition(row, col))
                _currentPoint = new ChessPoint(row,col);
            else
            {
                string message = String.Format("Vị trí quân {0} {1} tại hàng {2} cột {3} là không hợp lệ"
                    , ChessRule.GetTypeName(Type)
                    , ChessRule.GetSideName(Side)
                    , row
                    , col);
                throw new ExceptionInvalidChessPoint(message);
            }
        }
        private bool ValidatePosition(int row, int col)
        {
            if (row < 0 || row > ChessBoard.RowSize || col < 0 || col > ChessBoard.ColumnSize)
                return false;
            else
                return true;
        }
        public ChessPoint GetBoardPosition()
        {
            int row = (Side == BOARD_SIDE.RED) ? Row : ChessBoard.RowSize - Row + 1;
            int col = (Side == BOARD_SIDE.RED) ? Col : ChessBoard.ColumnSize - Col + 1;
            return new ChessPoint(row,col);
        }
        public List<ChessVector> GetScopeVector()
        {
            List<ChessVector> list = new List<ChessVector>();
            if (!IsAlive())
                return list;
            switch (Type)
            {
                case ITEM_TYPE.KING:
                    if (Row>1)
                        list.Add(ChessVector.MoveBackward()); //move backward
                    if (Row < 3)
                        list.Add(ChessVector.MoveForward()); //move forward
                    if (Col > 4)
                        list.Add(new ChessVector(0, -1)); //move left
                    if (Col < 6)
                        list.Add(new ChessVector(0,1)); //move right
                    break;
                case ITEM_TYPE.ADVISOR:
                    if (Row == 1) {
                        if (Col == 4)
                            list.Add(new ChessVector(1, 1));
                        else
                        if (Col == 6)
                            list.Add(new ChessVector(1, -1));
                    }
                    else
                    if (Row == 2)
                    {
                        if (Col == 5)
                        {
                            list.Add(new ChessVector(-1, -1));
                            list.Add(new ChessVector(-1, 1));
                            list.Add(new ChessVector(1, -1));
                            list.Add(new ChessVector(1, 1));
                        }
                    }
                    else
                    if (Row == 3)
                    {
                        if (Col == 4)
                            list.Add(new ChessVector(-1, 1));
                        else
                        if (Col == 6)
                            list.Add(new ChessVector(-1, -1));
                    }
                    break;
                case ITEM_TYPE.ELEPHANT:
                    if (Row == 1)
                    {
                        if (Col == 3 || Col ==7)
                        {
                            list.Add(new ChessVector(2,-2));
                            list.Add(new ChessVector(2,2));
                        }                        
                    }
                    else
                    if (Row == 3)
                    {
                        if (Col == 1)
                        {
                            list.Add(new ChessVector(2, 2));
                            list.Add(new ChessVector(-2, 2));
                        }
                        if (Col == 5)
                        {
                            list.Add(new ChessVector(-2, -2));
                            list.Add(new ChessVector(-2,  2));
                            list.Add(new ChessVector( 2, -2));
                            list.Add(new ChessVector( 2,  2));
                        }
                        else
                        if (Col == 9)
                        {
                            list.Add(new ChessVector(2, -2));
                            list.Add(new ChessVector(-2, -2));
                        }
                    }
                    else
                    if (Row == 5)
                    {
                        if (Col == 3 || Col == 7)
                        {
                            list.Add(new ChessVector(-2, -2));
                            list.Add(new ChessVector(-2, 2));
                        }
                    }
                    break;
                case ITEM_TYPE.CHARIOT:
                case ITEM_TYPE.CANON:
                    for (int r = 1; r < ChessBoard.RowSize; r++)
                    {
                        if (r != Row)
                        {
                            list.Add(new ChessVector(r - Row, 0));
                        }
                    }
                    for (int c = 1; c< ChessBoard.ColumnSize; c++)
                    {
                        if (c != Col)
                        {
                            list.Add(new ChessVector(0, c - Col));
                        }
                    }
                    break;
                
                case ITEM_TYPE.HORSE:
                    if (Row>=2)
                    {
                        if (Col < ChessBoard.ColumnSize)
                            list.Add(new ChessVector(-2, 1));
                        if (Col>1)
                            list.Add(new ChessVector(-2,-1));
                    }
                    if (Row <=ChessBoard.RowSize-2)
                    {
                        if (Col < ChessBoard.ColumnSize)
                            list.Add(new ChessVector(2,1));
                        if (Col > 1)
                            list.Add(new ChessVector(2,-1));
                    }
                    if (Col >= 2)
                    {
                        if (Row < ChessBoard.RowSize)
                            list.Add(new ChessVector(1,-2));
                        if (Row> 1)
                            list.Add(new ChessVector(-1,-2));
                    }
                    if (Col <=ChessBoard.ColumnSize-2)
                    {
                        if (Row < ChessBoard.RowSize)
                            list.Add(new ChessVector(1, 2));
                        if (Row > 1)
                            list.Add(new ChessVector(-1, 2));
                    }
                    break;                
                case ITEM_TYPE.PAWN:
                    if (Row<9)
                        list.Add(ChessVector.MoveForward()); 
                    if (Row > 5) // tốt đã qua sông
                    {
                        if (Col<ChessBoard.ColumnSize)
                            list.Add(ChessVector.MoveLeft());
                        if (Col>1)
                            list.Add(ChessVector.MoveRight());
                    }
                    break;
                default: break;
            }
            return list;
        }
    }
    public class ChessBoard
    {
        public const int RowSize = 10;
        public const int ColumnSize = 9;
        private int[,] _board;
        private List<ChessMan> _items;
        public Player Owner { get; }
        public Player RedPlayer { get; private set; }
        public Player BlackPlayer { get; private set; }        
        public List<Player> Observers { get; }
        public ChessBoard(Player owner)
        {
            _board = new int[RowSize,ColumnSize];
            
            _items = new List<ChessMan>();
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.KING,1,5));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.ADVISOR,1,4));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.ADVISOR,1,6));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.ELEPHANT,1,3));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.ELEPHANT,1,7));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.HORSE,1,2));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.HORSE,1,8));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.CANON,3,2));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.CANON,3,8));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.CHARIOT,1,1));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.CHARIOT,1,9));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.PAWN,4,1));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.PAWN,4,3));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.PAWN,4,5));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.PAWN,4,7));
            _items.Add(new ChessMan(this, BOARD_SIDE.RED, ITEM_TYPE.PAWN,4,9));

            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.KING, 1, 5));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.ADVISOR, 1, 4));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.ADVISOR, 1, 6));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.ELEPHANT, 1, 3));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.ELEPHANT, 1, 7));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.HORSE, 1, 2));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.HORSE, 1, 8));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.CANON, 3, 2));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.CANON, 3, 8));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.CHARIOT, 1, 1));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.CHARIOT, 1, 9));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.PAWN, 4, 1));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.PAWN, 4, 3));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.PAWN, 4, 5));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.PAWN, 4, 7));
            _items.Add(new ChessMan(this, BOARD_SIDE.BLACK, ITEM_TYPE.PAWN, 4, 9));

            Owner = owner;
            RedPlayer = owner; // mặc định người tạo bàn là người cầm quân đỏ
            Observers = new List<Player>();
        }
        public bool Join(Player player)
        {
            bool success = true;
            try
            {
                if (RedPlayer == null)
                    RedPlayer = player;
                else
                if (BlackPlayer == null)
                    BlackPlayer = player;
                else
                {
                    if (Observers.Find(item => item.Id == player.Id) == null)
                        Observers.Add(player);
                }
            }
            catch
            {
                success = false;
            }
            return success;
        }
        public bool CanSitDown()
        {
            return RedPlayer == null || BlackPlayer == null;
        }
        public bool SitDown(Player player)
        {
            bool success = true;

            if (RedPlayer == null)
            {
                RedPlayer = player;
            }
            else
            if (BlackPlayer == null)
            {
                BlackPlayer = player;
            }
            else
                success = false;
            if (success)
            {
                Observers.Remove(player);
            }
            return success;
        }
        public void ResetBoard()
        {
            ClearBoard();
            _items.ForEach(item => item.Reset());
        }
        public int[,] GetBoard()
        {
            int[,] result = new int[RowSize, ColumnSize];
            for(int r = 0; r<RowSize;r++)
                for(int c = 0; c < ColumnSize; c++)
                {
                    result[r, c] = _board[r, c];
                }
            return result ;
        }
        public bool IsRed(Player player)
        {
            return RedPlayer != null && player != null && RedPlayer.Id == player.Id;
        }
        public bool IsBlack(Player player)
        {
            return BlackPlayer != null && player != null && BlackPlayer.Id == player.Id;
        }
        public bool IsObserver(Player player)
        {
            return player != null && Observers.Exists(item => item.Id == player.Id);
        }
        public Player GetBlackPlayer()
        {
            return BlackPlayer;
        }
        public Player GetRedPlayer()
        {
            return RedPlayer;
        }
        public List<ChessVector> GetScope(ChessMan item)
        {
            List<ChessVector> list = new List<ChessVector>();
            if (!item.IsAlive())
                return list;
            list = item.GetScopeVector();
            List<ChessVector> removeList = new List<ChessVector>();
            switch (item.Type)
            {
                case ITEM_TYPE.KING:
                    list.ForEach(vector =>
                    {
                        //ChessMan checkItem = GetItemAt(item.GetBoardPosition(),vector);
                    });
                    break;
                default:
                    break;
            }
            return list;
        }

        public ChessMan MoveItem(Player player, ChessMan item, ChessPoint to)
        {
            throw new NotImplementedException();
        }

        public ChessMan PickItemUp(Player player, int row, int col)
        {
            throw new NotImplementedException();
        }

        public void SetBlackPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public void SetRedPlayer(Player player)
        {
            throw new NotImplementedException();
        }

        public void SwitchPlayer()
        {
            throw new NotImplementedException();
        }
        //===================================================================================================================
        private void PutChessMan(BOARD_SIDE side, ITEM_TYPE type, int row, int col)
        {
            ChessMan item = _items.Where(item => item.Side == side && item.Type == type && item.IsAlive()).FirstOrDefault();
            if (item == null)
            {
                string message = String.Format("Không tìm thấy quân {0} {1} nào để sắp quân vào bàn cờ."
                    , ChessRule.GetTypeName(type), ChessRule.GetSideName(side));
            }
            item.SetPosition(row, col);
        }
        private void ClearBoard()
        {
            _items.ForEach(item =>
            {
                item.SetPosition(0, 0);
            });
            _board = new int[RowSize, ColumnSize];
        }
        public void Update(ChessMan item)
        {
            ChessPoint position = item.GetBoardPosition();
            int side = (int)item.Side;
            int value = (int)item.Type;
            UpdatePosition(position, side * value);
        }
        private void UpdateBoard()
        {
            _items.ForEach(item =>
            {
                Update(item);
            });
        }
        private void UpdatePosition(ChessPoint position, int value)
        {
            _board[position.Row - 1, position.Col - 1] = value;
        }
        private void ClearPosition(ChessPoint position)
        {
            UpdatePosition(position, 0);
        }

    }

}
