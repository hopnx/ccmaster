using CCMaster.API.Domains;
using CCMaster.API.Models;
using CoreLibrary.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    public interface IBoardService
    {
        public Task<BaseResponse<DOBoard>> PlayGame(RequestPlayGame request);
        public void AddToNewBoardList(Board board);
        public void RemoveFromNewBoardList(Board board);

        public Board FindBoardByPlayerId(Guid playerId);
        public Task<BaseResponse<DOBoard>> GetBoardInfo(RequestGamePlay request);
        public Task<BaseResponse<DOBoard>> RejoinGame(Board board, Player player);
        public Board CreateBoard(Player owner);
        public Board GetBoard(Guid boardId);
        public Task SwitchSide(RequestGamePlay request);
        public Task ReadyToPlay(RequestGamePlay request);        
        public BaseResponse<DOBoardSnapshot> GetBoardSnapshot(RequestGamePlay request);
        public BaseResponse<DOBoard> JoinBoard(Board board, Player player);
        public BaseResponse<DOBoard> AssignBoard(Board board, Player player);
        public Task<BaseResponse<DOBoard>> LeaveBoard(RequestGamePlay request);

        public Task<BaseResponse<DOBoard>> CancelReadyPlay(RequestGamePlay request);
        public Task<BaseResponse<DOItem>> PickItem(RequestPickItem request);
        public Task MoveItem(RequestMoveItem request);
        public Task Resign(RequestGamePlay request);
        public Task DrawOffer(RequestGamePlay request);
        public Task AcceptDraw(RequestAcceptDraw request);
        public Task BlackWin(Board board,string description);
        public Task RedWin(Board board, string description);
        public Task<BaseResponse<BaseResult>> LeaveGame(RequestGamePlay request);
    }
}
