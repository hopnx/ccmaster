using CCMaster.API.Domains;
using CCMaster.API.Hubs;
using CCMaster.API.Models;
using CCMaster.API.Services.Interfaces;
using CoreLibrary.Base;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using MongoDB.Driver.Core.WireProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    static public partial class EntityExtension
    {
        static public DOBoardShortcut MapToShortcut(this Board data)
        {
            return new DOBoardShortcut
            {
                Id = data.Id,
                RedPlayerName = data.RedPlayer?.Profile?.Name,
                BlackPlayerName = data.BlackPlayer?.Profile?.Name
            };
        }
        static public DOBoardShortcut MapToShortcut(this DOBoard data)
        {
            return new DOBoardShortcut
            {
                Id = data.Id,
                RedPlayerName = data.RedPlayer?.Name,
                BlackPlayerName = data.BlackPlayer?.Name
            };
        }
        static public PlayerInBoard MapToPlayerInBoard(this Player data)
        {
            return new PlayerInBoard
            {
                Id = data.Id,
                Profile = data,
            };
        }
        static public DOPlayerInBoard MapToPlayerInBoard(this PlayerInBoard data)
        {
            return new DOPlayerInBoard
            {
                Id = data.Id,
                Name = data.Profile.Name,
                Rank = data.Profile.RankLabel,
                Score = data.Profile.Score,
                ReadyToPlay = data.ReadyToPlay,
                TotalTime = data.TotalTime,
                TotalMoveTime = data.TotalMoveTime,
                RemainTime = data.RemainTime,
                RemainMoveTime = data.RemainMoveTime,
                IsYourTurn = data.IsYourTurn,
            };
        }

        static public DOPlayer MapTo(this Player data)
        {
            return new DOPlayer
            {
                Id = data.Id,
                Name = data.Name,
                RankLabel = data.RankLabel,
                RankIndex = data.RankIndex,
                StarIndex = data.StarIndex,
                Score = data.Score,
                Coin = data.Coin,
                TotalGame = data.TotalGame,
                TotalWin = data.TotalWin,
                TotalDraw = data.TotalDraw,
                TotalLose = data.TotalLose,
            };
        }
        static public DOBoard MapTo(this Board data)
        {
            if (!data.IsGameOver && data.RedPlayer != null)
                data.UpdateRemainTime(data.RedPlayer, DateTime.Now);

            if (!data.IsGameOver && data.BlackPlayer != null)
                data.UpdateRemainTime(data.BlackPlayer, DateTime.Now);

            DOBoard result = new DOBoard
            {
                Id = data.Id,
                Owner = data.Owner?.MapToPlayerInBoard(),
                RedPlayer = data.RedPlayer?.MapToPlayerInBoard(),
                BlackPlayer = data.BlackPlayer?.MapToPlayerInBoard(),
                Status = data.Status,
                TotalTime = data.TotalTime,
                AddingTime = 0,
                Result = data.Result,
                Turn = data.Turn,
                Items = new List<DOItem>(),

            };
            if (data.Items != null)
            {
                data.Items.ForEach(item =>
                {
                    result.Items.Add(item.MapTo(false));
                });
            }
            return result;
        }
        static public DOBoardSnapshot MapToSnapshot(this Board data)
        {
            DOBoardSnapshot result = new DOBoardSnapshot
            {
                Id = data.Id,
                Turn = data.Turn,
                Items = new List<DOItem>(),
            };
            if (data.Items != null)
            {
                data.Items.ForEach(item =>
                {
                    result.Items.Add(item.MapTo(false));
                });
            }
            return result;
        }
        static public DOItem MapTo(this Item data, bool includeScopes = true)
        {
            DOItem result =  new DOItem
            {
                Color = data.Color,
                Type = data.Type,
                Row = data.Row,
                Col = data.Col,
                Alive = data.IsAlive,
            };
            if (includeScopes)
            {
                result.Scope = new List<DOPosition>();
                data.Scopes.ForEach(p => result.Scope.Add(new DOPosition { Row = p.Row, Col = p.Col }));
            }
            return result;
        }
    }
    public class CheckResult
    {
        public const string OK = "OK";
    }

    public class BoardService : BaseService, IBoardService
    {
        private readonly IHubContext<BoardHub> _boardHub;
        private readonly IPlayerService _playerService;
        private Dictionary<Guid, Board> _mapBoard;
        private Dictionary<Guid, Board> _mapPlayerInBoard;
        public BoardService(IDistributedCache cache
            , IMongoConfiguration mongoConfiguration
            , IPlayerService playerService
            , IHubContext<BoardHub> boardHub) : base(cache, mongoConfiguration)
        {
            _playerService = playerService;
            _boardHub = boardHub;
            _mapBoard = new Dictionary<Guid, Board>();
            _mapPlayerInBoard = new Dictionary<Guid, Board>();

            _dicNewBoards = new Dictionary<Guid, Board>();
            _dicBoards = new Dictionary<Guid, Board>();

        }

        public readonly string DM_DEFAULT_ROOM = "Default";
        public readonly string DM_NEW_LIST = "NewList";

        private readonly Dictionary<Guid, Board> _dicNewBoards;
        private readonly Dictionary<Guid, Board> _dicBoards;

        public async Task<BaseResponse<BaseResult>> CreateGame(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                Player player = await _playerService.GetPlayer(request.PlayerId);
                if (player == null)
                {
                    response.Message = "Người chơi không tồn tại";
                    return response;
                }
                Board board = CreateBoard(player);
                if (board == null)
                {
                    response.Message = "Lỗi: không tạo được bàn chơi mới";
                    return response;
                }
                AddToNewBoardList(board);
                response.Data.Id = board.Id;
                response.OK = true;
            }
            catch(Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public async Task<BaseResponse<BaseResult>> FindGame(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                Player player = await _playerService.GetPlayer(request.PlayerId);
                if (player == null)
                {
                    response.Message = "Người chơi không tồn tại";
                    return response;
                }
                if (_dicNewBoards.Count > 0)
                {
                //    Board board = 
                }
                Board board = CreateBoard(player);
                if (board == null)
                {
                    response.Message = "Lỗi: không tạo được bàn chơi mới";
                    return response;
                }
                AddToNewBoardList(board);
                response.Data.Id = board.Id;
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }

        public async Task<BaseResponse<DOBoard>> PlayGame(RequestGamePlay request)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                Player player = await _playerService.GetPlayer(request.PlayerId);
                if (player == null)
                {
                    response.Message = "Người chơi không tồn tại";
                    return response;
                }
                Board board = FindBoardByPlayerId(request.PlayerId);
                if (board != null)
                {
                    response = await RejoinGame(board, player);
                    return response;
                }
                // get list board
                if (_dicNewBoards.Count == 0)
                {
                    board = CreateBoard(player);
                    if (board == null)
                    {
                        response.Message = "Lỗi: không tạo được bàn chơi mới";
                        return response;
                    }
                    AddToNewBoardList(board);
                    response.Data = board.MapTo();
                }
                else
                {
                    board = _dicNewBoards.Values.FirstOrDefault();
                    BaseResponse<DOBoard> result = AssignBoard(board, player);
                    if (!result.OK)
                    {
                        response.Message = result.Message;
                    }
                    else
                    {
                        if (result.Data.RedPlayer != null && result.Data.BlackPlayer != null)
                        {
                            RemoveFromNewBoardList(board);
                            AddToBoardList(board);
                        }
                        response.Data = board.MapTo();
                        response.OK = true;
                    }
                }

                response.OK = true;

            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public void RemoveFromNewBoardList(Board board)
        {
            _dicNewBoards.Remove(board.Id);
        }
        public void AddToNewBoardList(Board board)
        {
            if (!_dicNewBoards.ContainsKey(board.Id))
                _dicNewBoards.Add(board.Id, board);
        }
        public void RemoveFromBoardList(Board board)
        {
            _dicBoards.Remove(board.Id);
        }
        public void AddToBoardList(Board board)
        {
            if (!_dicBoards.ContainsKey(board.Id))
                _dicBoards.Add(board.Id, board);
        }


        public Board FindBoardByPlayerId(Guid playerId)
        {
            return _mapPlayerInBoard.GetValueOrDefault(playerId);
        }

        public async Task<BaseResponse<DOBoard>> GetBoardInfo(RequestGamePlay request)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                Player player = await _playerService.GetPlayer(request.PlayerId);
                if (player == null)
                {
                    response.Message = "Không tìm thấy người chơi";
                    return response;
                }
                Board board = GetBoard(request.BoardId);
                if (board == null)
                {
                    response.Message = "Không tìm thấy thông tin bàn chơi";
                    return response;
                }
                board.SaveConnection(request.PlayerId, request.ConnectionId);
                response.Data = board.MapTo();
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        private void SaveBoard(Board board)
        {
            if (board != null && !_mapBoard.ContainsKey(board.Id))
            {
                _mapBoard.Add(board.Id, board);
            }
        }
        public Board CreateBoard(Player owner)
        {
            Board board = new Board(owner, this);
            SaveBoard(board);
            RegisterPlayerInBoard(board, owner);
            _playerService.SavePlayer(owner);
            return board;
        }
        public void RegisterPlayerInBoard(Board board, Player player)
        {
            if (player != null)
            {
                UnregisterPlayerInBoard(player.Id);
                _mapPlayerInBoard.Add(player.Id, board);
            }
        }
        public void UnregisterPlayerInBoard(Player player)
        {
            if (player != null)
                UnregisterPlayerInBoard(player.Id);
        }
        public void UnregisterPlayerInBoard(Guid playerId)
        {
            _mapPlayerInBoard.Remove(playerId);
        }
        public BaseResponse<DOBoard> JoinBoard(Board board, Player player)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                if (board.RedPlayer != null && board.BlackPlayer != null)
                {
                    if (!board.Observers.ContainsKey(player.Id))
                    {
                        board.Observers.Add(player.Id, player.MapToPlayerInBoard());
                        response.OK = true;
                    }
                }
                else
                {
                    response.OK = board.AssignPlayer(player);
                }
                if (response.OK)
                {
                    SaveBoard(board);
                    _playerService.SavePlayer(player);
                }
                response.Data = board.MapTo();
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public BaseResponse<DOBoard> AssignBoard(Board board, Player player)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                // check if ready assign
                if ((board.RedPlayer != null && board.RedPlayer.Id == player.Id) ||
                    (board.BlackPlayer != null && board.BlackPlayer.Id == player.Id)
                    )
                {
                    response.OK = true;
                    return response;
                }
                if (board.RedPlayer != null && board.BlackPlayer != null)
                {
                    response.Message = "Bàn chơi đã đủ người";
                    return response;
                }
                else
                {
                    response.OK = board.AssignPlayer(player);
                    if (!response.OK)
                    {
                        response.Message = "Không thể gia nhập bàn chơi";
                        return response;
                    }
                    else
                    {
                        RegisterPlayerInBoard(board, player);
                    }
                }
                if (response.OK)
                {
                    if (board.BlackPlayer != null && board.RedPlayer != null && board.Status == BoardStatus.NEW)
                    {
                        board.Status = BoardStatus.READY;
                    }
                    SaveBoard(board);
                }
                response.Data = board.MapTo();
                response.OK = true;
                NotifyForAll(board, response, BoardHub.RECEIVE_PLAYER_JOIN);
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public Board GetBoard(Guid boardId)
        {
            Board board = _mapBoard.GetValueOrDefault(boardId);
            return board;
        }
        public BaseResponse<DOBoardSnapshot> GetBoardSnapshot(RequestGamePlay request)
        {
            BaseResponse<DOBoardSnapshot> response = CreateResponse<DOBoardSnapshot>();
            try
            {
                Board board = GetBoard(request.BoardId);
                response.Data = board.MapToSnapshot();
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public async Task SwitchSide(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                var check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_SWITCH_SIDE);
                    return;
                }
                Board board = GetBoard(request.BoardId);
                if (board.RedPlayer == null || board.RedPlayer.Id != request.PlayerId)
                {
                    response.Message = "Chỉ người cầm bên Đỏ mới được phép chấp tiên";
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_SWITCH_SIDE);
                    return;
                }
                board.SwitchSide();
                SaveBoard(board);
                response.OK = true;
                BaseResponse<DOBoard> res = CreateResponse<DOBoard>();
                res.OK = true;
                res.Data = board.MapTo();
                NotifyForAll(board, res, BoardHub.RECEIVE_BOARD_INFO);
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
        }
        public async Task<BaseResponse<DOBoard>> LeaveBoard(RequestGamePlay request)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    return response;
                }
                Player player = await _playerService.GetPlayer(request.PlayerId);
                Board board = GetBoard(request.BoardId);

                if ((board.RedPlayer != null && board.RedPlayer.Id == player.Id) ||
                    (board.BlackPlayer != null && board.BlackPlayer.Id == player.Id))
                {
                    if (board.Status == BoardStatus.PLAYING)
                    {
                        response.Message = "Bạn không thể rời bàn trong lúc đang chơi";
                        return response;
                    }
                }
                // leave if player is observer
                board.Observers.Remove(player.Id);
                if (board.RedPlayer.Id == player.Id)
                    board.SetPlayer(Color.RED, null);
                else
                if (board.BlackPlayer.Id == player.Id)
                {
                    board.SetPlayer(Color.BLACK, null);
                }
                SaveBoard(board);

                _playerService.SavePlayer(player);
                board.RemoveConnection(request.PlayerId);

                response.Data = board.MapTo();
                response.OK = true;

                NotifyForAll(board, response, "ReceivePlayerLeave");
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        private async Task<BaseResponse<BaseResult>> CheckGamePlayRequest(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            Player player = await _playerService.GetPlayer(request.PlayerId);
            if (player == null)
            {
                response.Message = "Người chơi không tồn tại";
                return response;
            }
            Board board = GetBoard(request.BoardId);
            if (board == null)
            {
                response.Message = "Bàn chơi không tồn tại";
                return response;
            }
            response.OK = true;
            return response;
        }
        public async Task ReadyToPlay(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_READY_TO_PLAY);
                    return;
                }
                Board board = GetBoard(request.BoardId);
                if (board.RedPlayer.Id == request.PlayerId)
                {
                    board.RedPlayer.ReadyToPlay = true;
                }
                else
                if (board.BlackPlayer.Id == request.PlayerId)
                {
                    board.BlackPlayer.ReadyToPlay = true;
                }
                if (board.RedPlayer != null &&
                    board.RedPlayer.ReadyToPlay &&
                    board.BlackPlayer != null &&
                    board.BlackPlayer.ReadyToPlay 
                )
                {
                    board.Status = BoardStatus.START_GAME;
                    board.Reset();
                    SaveBoard(board);
                    SendBoardToAll(board, BoardHub.RECEIVE_START_GAME);
                    SendBoardToAll(board, BoardHub.RECEIVE_BOARD_INFO);
                }
                else
                {
                    SaveBoard(board);
                    SendBoardToAll(board, BoardHub.RECEIVE_BOARD_INFO);
                }
                response.OK = true;

            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
        }
        private void SendBoardToAll(Board board, string command)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            response.Data = board.MapTo();
            response.OK = true;
            NotifyForAll(board, response, command);
        }
        private void NotifyForAll(Board board, object response, string command)
        {
            board.ConnectionByPlayer.Values.ToList().ForEach(connectionId =>
            {
                _ = _boardHub.Clients.Clients(connectionId).SendAsync(command, response);
            });
        }
        private void NotifyForOne(string connectionId, object response, string command)
        {
            _ = _boardHub.Clients.Clients(connectionId).SendAsync(command, response);
        }
        public async Task<BaseResponse<DOBoard>> CancelReadyPlay(RequestGamePlay request)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    return response;
                }
                Board board = GetBoard(request.BoardId);
                if (board.RedPlayer.Id == request.PlayerId)
                {
                    board.RedPlayer.ReadyToPlay = false;
                }
                else
                if (board.BlackPlayer.Id == request.PlayerId)
                {
                    board.BlackPlayer.ReadyToPlay = false;
                }

                SaveBoard(board);
                response.Data = board.MapTo();
                response.OK = true;
                NotifyForAll(board, response, "ReceiveCancelReadyPlay");
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public async Task<BaseResponse<DOItem>> PickItem(RequestPickItem request)
        {
            BaseResponse<DOItem> response = CreateResponse<DOItem>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    return response;
                }
                response.OK = true;
                Board board = GetBoard(request.BoardId);

                if (board.Turn != request.Color)
                {
                    return response; // not your turn
                }
                if (board.Mode == PlayMode.MODE_PLAYER_VS_PLAYER)
                {
                    if ((board.RedPlayer != null && board.RedPlayer.Id == request.PlayerId && request.Color == Color.BLACK) ||
                        (board.BlackPlayer != null && board.BlackPlayer.Id == request.PlayerId && request.Color == Color.RED))
                    {
                        return response; // unselected
                    }
                }

                Item item = board.PickItem(
                request.Color,
                request.Type,
                request.Row,
                request.Col);
                if (item == null)
                {
                    response.Message = String.Format("Không tìm thấy quân cờ {0} {1} tại vị trí ({2},{3}",
                        request.Color,
                        request.Type,
                        request.Row,
                        request.Col
                        );
                    return response;
                }
                //re-calculate scope
                //board.CalculateScope(item);
                response.Data = item.MapTo();
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public async Task MoveItem(RequestMoveItem request)
        {
            BaseResponse<DOMove> response = CreateResponse<DOMove>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Code = "INVALID_REQUEST";
                    response.Message = check.Message;
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
                    return;
                }
                Board board = GetBoard(request.BoardId);
                if (board.Mode == PlayMode.MODE_PLAYER_VS_PLAYER)
                {
                    if ((board.RedPlayer.Id == request.PlayerId && request.FromColor == Color.BLACK) ||
                        (board.BlackPlayer.Id == request.PlayerId && request.FromColor == Color.RED))
                    {
                        response.Code = "INVALID_REQUEST_DATA";
                        response.Message = "Dữ liệu yêu cầu nước đi không hợp lệ";
                        NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
                        return;
                    }
                }
                if (board.Turn != request.FromColor)
                {
                    response.Code = "NOT_YOUR_TURN";
                    response.Message = "Chưa đến lượt bạn đi";
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
                    return;
                }
                Item item = board.PickItem(
                request.FromColor,
                request.FromType,
                request.FromRow,
                request.FromCol);

                if (item == null)
                {
                    response.Code = "ITEM_NOT_FOUND";
                    response.Message = String.Format("Không tìm thấy quân cờ {0} {1} tại vị trí ({2},{3}",
                        request.FromColor,
                        request.FromType,
                        request.FromRow,
                        request.FromCol
                        );
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
                    return;
                }
                //                 
                Position targetPosition = new Position(request.ToRow, request.ToCol);
                if (!item.Scopes.Contains(targetPosition))
                {
                    response.Code = "INVALID_MOVE";
                    response.Message = "Nước đi không hợp lệ do nằm ngoài phạm vi cho phép";
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
                    return;
                }

                response.Data.Item = new DOItem
                {
                    Alive = true,
                    Type = request.FromType,
                    Color = request.FromColor,
                    Row = request.FromRow,
                    Col = request.FromCol,
                };

                Item target = board.FindItem(request.FromColor, request.ToRow, request.ToCol);
                if (target != null)
                    response.Data.Kill = target.MapTo();
                response.Data.Destination = targetPosition;

                board.MoveItemTo(item, targetPosition);
                board.AnalyseBoard();
                SaveBoard(board);

                response.OK = true;
                response.Data.Turn = board.Turn;
                response.Data.RedPlayer = board.RedPlayer.MapToPlayerInBoard();
                response.Data.BlackPlayer = board.BlackPlayer.MapToPlayerInBoard();
                response.Data.IsCheckMate = board.IsCheckMated;
                response.Data.IsGameOver = board.IsGameOver;
                response.Data.BoardStatus = board.Status;
                response.Data.WarningSide = board.WarningSide;
                response.Data.WarningMessage = board.WarningMessage;
                NotifyForAll(board, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);

                if (board.IsGameOver)
                    await GameOver(board);
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
                NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_PLAYER_MOVE_ITEM);
            }
        }
        public async Task GameOver(Board board)
        {
            if (board.Turn == Color.RED)
                await BlackWin(board, board.WarningMessage);
            else
            if (board.Turn == Color.BLACK)
                await RedWin(board, board.WarningMessage);
        }
        public async Task Resign(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    response.Message = check.Message;
                    NotifyForOne(request.ConnectionId, response, BoardHub.RECEIVE_RESIGN);
                    return;
                }

                Board board = GetBoard(request.BoardId);
                if (board.RedPlayer != null && board.RedPlayer.Id == request.PlayerId)
                    await BlackWin(board, "Bên ĐỎ chấp nhận đầu hàng");
                else
                if (board.BlackPlayer != null && board.BlackPlayer.Id == request.PlayerId)
                    await RedWin(board, "Bên ĐEN chấp nhận đầu hàng");
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
        }
        public async Task DrawOffer(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = CreateResponse<BaseResult>();
            try
            {
                BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
                if (!check.OK)
                {
                    return;
                }

                Board board = GetBoard(request.BoardId);
                response.OK = true;
                if (board.RedPlayer != null && request.PlayerId == board.RedPlayer.Id)
                {
                    string connectionId = board.ConnectionByPlayer.GetValueOrDefault(board.BlackPlayer.Id);
                    NotifyForOne(connectionId, response, BoardHub.RECEIVE_DRAW_OFFER);
                }
                else
                if (board.BlackPlayer != null && request.PlayerId == board.BlackPlayer.Id)
                {
                    string connectionId = board.ConnectionByPlayer.GetValueOrDefault(board.RedPlayer.Id);
                    NotifyForOne(connectionId, response, BoardHub.RECEIVE_DRAW_OFFER);
                }
                response.OK = true;
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
        }
        public async Task AcceptDraw(RequestAcceptDraw request)
        {
            if (!request.Accept)
                return;
            BaseResponse<BaseResult> check = await CheckGamePlayRequest(request);
            if (!check.OK)
            {
                return;
            }
            Board board = GetBoard(request.BoardId);
            await Draw(board, "Hai bên đồng ý HÒA");
            return;
        }
        public async Task<BaseResponse<DOBoard>> RejoinGame(Board board, Player player)
        {
            BaseResponse<DOBoard> response = CreateResponse<DOBoard>();
            try
            {
                if (board == null)
                {
                    response.Message = "Không có thông tin bàn chơi";
                    return response;
                }
                if (player == null)
                {
                    response.Message = "Không có thông tin người chơi";
                    return response;
                }
                if ((board.RedPlayer != null && board.RedPlayer.Id == player.Id) ||
                    (board.BlackPlayer != null && board.BlackPlayer.Id == player.Id
                   ))
                {
                    response.OK = true;
                    response.Data = board.MapTo();
                    NotifyForAll(board, response, BoardHub.RECEIVE_PLAYER_JOIN);
                }
            }
            catch (Exception e)
            {
                ExceptionHandle(response, e);
            }
            return response;
        }
        public async Task BlackWin(Board board, string description)
        {
            BaseResponse<DOGameOver> response = CreateResponse<DOGameOver>();
            response.OK = true;
            board.Status = Board.LOSE;
            Player redPlayer = await _playerService.GetPlayer(board.RedPlayer.Id);
            Player blackPlayer = await _playerService.GetPlayer(board.BlackPlayer.Id);
            redPlayer.AddLose(GameConfig.BONUS_SCORE, board.BetCoin);
            blackPlayer.AddWin(GameConfig.BONUS_SCORE, board.BetCoin);
            _playerService.SavePlayer(redPlayer);
            _playerService.SavePlayer(blackPlayer);

            response.Data.Result = board.Status;//black win 
            /*
            response.Data.RedPlayer = redPlayer.MapTo();
            response.Data.RedPlayer.Score = -GameConfig.BONUS_SCORE;

            response.Data.BlackPlayer = blackPlayer.MapTo();
            response.Data.BlackPlayer.Score = GameConfig.BONUS_SCORE;
            response.Data.Description = description;
            */
            Reset(board);
            NotifyForAll(board, response, BoardHub.RECEIVE_GAME_OVER);
        }
        public async Task RedWin(Board board, string description)
        {
            BaseResponse<DOGameOver> response = CreateResponse<DOGameOver>();
            response.OK = true;
            board.Status = Board.WIN;
            Player redPlayer = await _playerService.GetPlayer(board.RedPlayer.Id);
            Player blackPlayer = await _playerService.GetPlayer(board.BlackPlayer.Id);
            redPlayer.AddWin(GameConfig.BONUS_SCORE, board.BetCoin);
            blackPlayer.AddLose(GameConfig.BONUS_SCORE, board.BetCoin);
            _playerService.SavePlayer(redPlayer);
            _playerService.SavePlayer(blackPlayer);

            response.Data.Result = board.Status;

            /*
            response.Data.RedPlayer = redPlayer.MapTo();
            response.Data.RedPlayer.Score = GameConfig.BONUS_SCORE;

            response.Data.BlackPlayer = blackPlayer.MapTo();
            response.Data.BlackPlayer.Score = -GameConfig.BONUS_SCORE;
            response.Data.Description = description;
            */
            Reset(board);
            NotifyForAll(board, response, BoardHub.RECEIVE_GAME_OVER);
        }
        public async Task Draw(Board board, string description)
        {
            BaseResponse<DOGameOver> response = CreateResponse<DOGameOver>();
            response.OK = true;
            board.Status = Board.DRAW;

            response.Data.Result = board.Status;
            Player redPlayer = await _playerService.GetPlayer(board.RedPlayer.Id);
            redPlayer.AddDraw();
            _playerService.SavePlayer(redPlayer);
            /*
            response.Data.RedPlayer = redPlayer.MapTo();
            response.Data.RedPlayer.Score = 0;

            Player blackPlayer = await _playerService.GetPlayer(board.BlackPlayer.Id);
            blackPlayer.AddDraw();
            _playerService.SavePlayer(blackPlayer);
            response.Data.BlackPlayer = blackPlayer.MapTo();
            response.Data.BlackPlayer.Score = 0;
            response.Data.Description = description;
            */
            Reset(board);
            NotifyForAll(board, response, BoardHub.RECEIVE_GAME_OVER);
        }
        public void Reset(Board board)
        {
            board.Reset();
            board.Status = BoardStatus.READY;
            board.SwitchSide();
            if (board.RedPlayer != null)
            {
                board.RedPlayer.ReadyToPlay = false;
                board.RedPlayer.IsYourTurn = false;
                board.RedPlayer.StartThinking = false;
            }
            if (board.BlackPlayer != null)
            {
                board.BlackPlayer.ReadyToPlay = false;
                board.BlackPlayer.IsYourTurn = false;
                board.BlackPlayer.StartThinking = false;                
            }
            SaveBoard(board);
        }
        public async Task<BaseResponse<BaseResult>> LeaveGame(RequestGamePlay request)
        {
            BaseResponse<BaseResult> response = await CheckGamePlayRequest(request);
            try
            {
                if (!response.OK)
                {
                    return response;
                }
                Board board = GetBoard(request.BoardId);
                if (board.Status == BoardStatus.PLAYING)
                {
                    if (board.IsPlayer(request.PlayerId))
                    {
                        if (board.IsRedPlayer(request.PlayerId))
                            await BlackWin(board, "Bên ĐỎ đã rời bàn chơi");
                        else
                        if (board.IsBlackPlayer(request.PlayerId))
                            await RedWin(board, "Bên ĐEN đã rời bàn chơi");
                    }
                }
                board.UnAssigned(request.PlayerId);
                RemovePlayerInBoard(request.PlayerId);
                if (board.RedPlayer == null && board.BlackPlayer == null && board.Observers.Count == 0)
                    RemoveBoard(board.Id);
                else
                if (!board.IsGameOver)
                {
                    BaseResponse<DOBoard> res = CreateResponse<DOBoard>();
                    res.OK = true;
                    res.Data = board.MapTo();
                    NotifyForAll(board, res, BoardHub.RECEIVE_BOARD_INFO);
                    AddToNewBoardList(board);
                }

            }
            catch (Exception e)
            {
                response.OK = false;
                response.Message = e.Message;
            }
            return response;
        }
        private void RemoveBoard(Guid boardId)
        {
            _mapBoard.Remove(boardId);
        }
        private void RemovePlayerInBoard(Guid playerId)
        {
            _mapPlayerInBoard.Remove(playerId);
        }
    }
}
