using CCMaster.API.Domains;
using CCMaster.API.Models;
using CCMaster.API.Services;
using CCMaster.API.Services.Interfaces;
using CoreLibrary.Base;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CCMaster.API.Hubs
{
    public interface IBoardHub
    {
        Task ReceivePlayGame(BaseResponse<DOBoard> response);
        public Task ReplyBoardInfo(BaseResponse<DOBoard> response);
        public Task ReceiveSwitchSide(BaseResponse<BaseResult> response);
        public Task ReceiveReadyToPlay(BaseResponse<BaseResult> response);
        public Task ReplyPickItem(BaseResponse<DOItem> response);
        public Task ReceiveMoveItem(BaseResponse<DOMove> response);
        public Task ReplyBoardSnapshot(BaseResponse<DOBoardSnapshot> response);
        public Task ReplyLeaveGame(BaseResponse<BaseResult> response);
        public Task ReplyResign(BaseResponse<BaseResult> response);
        public Task ReplyDrawOffer(BaseResponse<BaseResult> response);
        public Task ReceiveDrawOffer(BaseResponse<BaseResult> response);


    }
    public class BoardHub : Hub<IBoardHub>
    {
        static readonly public string Route = "hubs/board";
        public const string RECEIVE_BOARD_INFO = "ReceiveBoardInfo";
        public const string RECEIVE_SWITCH_SIDE = "ReceiveSwitchSide";
        public const string RECEIVE_READY_TO_PLAY = "ReceiveReadyToPlay";
        public const string RECEIVE_START_GAME = "ReceiveStartGame";
        public const string RECEIVE_RESIGN = "ReceiveResign";
        public const string RECEIVE_DRAW_OFFER = "ReceiveDrawOffer";

        public const string RECEIVE_PLAYER_JOIN = "ReceivePlayerJoin";
        public const string RECEIVE_PLAYER_LEAVE = "ReceivePlayerLeave";
        public const string RECEIVE_PLAYER_MOVE_ITEM = "ReceiveMoveItem";
        public const string RECEIVE_GAME_OVER = "ReceiveGameOver";
        public const string REPLY_LEAVE_GAME = "ReplyLeaveGame";

        private readonly IBoardService _service;
        public BoardHub(IBoardService service)
        {
            _service = service;
        }

        public async Task RequestPlayGame(RequestPlayGame request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOBoard> response = await _service.PlayGame(request);
            _ = Clients.Clients(request.ConnectionId).ReceivePlayGame(response);
        }

        public async Task RequestBoard(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOBoard> response = await _service.GetBoardInfo(request);
            _ = Clients.Clients(request.ConnectionId).ReplyBoardInfo(response);
        }
        public void RequestBoardSnapshot(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOBoardSnapshot> response = _service.GetBoardSnapshot(request);
            _ = Clients.Clients(request.ConnectionId).ReplyBoardSnapshot(response);
        }
        public async Task RequestSwitchSide(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.SwitchSide(request);
        }       
        public async Task RequestReadyToPlay(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.ReadyToPlay(request);
        }     
        public async Task RequestPickItem(RequestPickItem request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<DOItem> response = await _service.PickItem(request);
            _ = Clients.Clients(request.ConnectionId).ReplyPickItem(response);
        }
        public async Task RequestMoveItem(RequestMoveItem request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.MoveItem(request);
        }             
        public async Task RequestResign(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.Resign(request);
        }
        public async Task RequestDraw(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.DrawOffer(request);
        }
        public async Task RequestAcceptDraw(RequestAcceptDraw request)
        {
            request.ConnectionId = Context.ConnectionId;
            await _service.AcceptDraw(request);
        }
        public async Task RequestLeaveGame(RequestGamePlay request)
        {
            request.ConnectionId = Context.ConnectionId;
            BaseResponse<BaseResult> response = await _service.LeaveGame(request);
            _ = Clients.Clients(request.ConnectionId).ReplyLeaveGame(response);
        }
    }
}
