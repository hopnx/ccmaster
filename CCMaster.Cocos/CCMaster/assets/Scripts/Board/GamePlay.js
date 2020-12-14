let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');
let ChessDefinition = require("ChessDefinition");
var GamePlay = cc.Class({
    extends: cc.Component,
    statics: {
        url: GLOBAL.Host + '/hubs/board',
        commands: {
            REQUEST_BOARD: "RequestBoard",
            REQUEST_SWITCH_SIDE: "RequestSwitchSide",
            REQUEST_READY_TO_PLAY: 'RequestReadyToPlay',
            REQUEST_PICK_ITEM: 'RequestPickItem',
            REQUEST_MOVE_ITEM: 'RequestMoveItem',
            REQUEST_BOARD_SNAPSHOT: "RequestBoardSnapshot",
            REQUEST_RESIGN: "RequestResign",
            REQUEST_DRAW: "RequestDraw",
            REQUEST_ACCEPT_DRAW: "RequestAcceptDraw",
            REQUEST_LEAVE_GAME: "RequestLeaveGame",
        },
        events: {
            RECEIVE_BOARD_INFO: 'ReceiveBoardInfo',
            REPLY_BOARD_INFO: 'ReplyBoardInfo',
            REPLY_BOARD_SNAPSHOT: 'ReplyBoardSnapshot',
            REPLY_SWITCH_SIDE: 'ReplySwitchSide',
            REPLY_READY_TO_PLAY: 'ReplyReadyToPlay',
            REPLY_PICK_ITEM: 'ReplyPickItem',
            REPLY_MOVE_ITEM: 'ReplyMoveItem',
            REPLY_LEAVE_GAME: 'ReplyLeaveGame',
            REPLY_RESIGN: 'ReplyResign',
            REPLY_DRAW_OFFER: 'ReplyDrawOffer',

            RECEIVE_PLAYER_JOIN: "ReceivePlayerJoin",
            RECEIVE_PLAYER_LEAVE: "ReceivePlayerLeave",
            RECEIVE_START_GAME: "ReceiveStartGame",
            RECEIVE_MOVE_ITEM: "ReceiveMoveItem",
            RECEIVE_GAME_OVER: "ReceiveGameOver",
            RECEIVE_DRAW_OFFER: "ReceiveDrawOffer",
            REPLY_LEAVE_GAME: "ReplyLeaveGame",
        }
    },
    properties: {
        nodeBoard: cc.Node,
        nodeFirstPlayer: cc.Node,
        nodeSecondPlayer: cc.Node,
        nodeNotify: cc.Node,
        nodeLeftMenu: cc.Node,
        nodeStartGame: cc.Node,
        nodeCheckMate: cc.Node,
        nodeGameOver: cc.Node,
        nodeDrawOffer: cc.Node,
        nodeDebug: cc.Node,
    },
    debug(object) {
        if (this.nodeDebug) {
            let text = JSON.stringify(object);
            this.nodeDebug.getComponent(cc.Label).string = text;
            this.nodeDebug.active = true;
        }
    },
    onLoad() {
        this.leftMenu = this.nodeLeftMenu.getComponent("LeftMenu");
        this.board = this.nodeBoard.getComponent("Board");
        this.firstPlayer = this.nodeFirstPlayer.getComponent("Player");
        this.secondPlayer = this.nodeSecondPlayer.getComponent("Player");
        this.startGame = this.nodeStartGame.getComponent("Flash");
        this.checkMate = this.nodeCheckMate.getComponent("Flash");
        this.gameOver = this.nodeGameOver.getComponent("GameOver");
        this.drawOffer = this.nodeDrawOffer.getComponent("DrawOffer");
        this.notify = this.nodeNotify.getComponent("Flash");

        if (Global.account && Global.account.player) {
            this.ownerId = Global.account.player.id;
        }
        else
            this.ownerId = null;
        if (Global.board) {
            this.boardId = Global.board.id;
        }
        else {
            this.boardId = null;
        }
        this.requestGamePlay = {
            boardId: this.boardId,
            playerId: this.ownerId,
        }

        let listener = [
            { event: GamePlay.events.REPLY_BOARD_INFO, handler: this.receiveBoardInfo.bind(this) },
            { event: GamePlay.events.RECEIVE_BOARD_INFO, handler: this.receiveBoardInfo.bind(this) },
            { event: GamePlay.events.BOARD_SNAPSHOT, handler: this.receiveBoardSnapshot.bind(this) },

            { event: GamePlay.events.REPLY_SWITCH_SIDE, handler: this.receiveResult.bind(this) },
            { event: GamePlay.events.RECEIVE_START_GAME, handler: this.receiveStartGame.bind(this) },
            { event: GamePlay.events.REPLY_PICK_ITEM, handler: this.receivePickItem.bind(this) },
            { event: GamePlay.events.RECEIVE_MOVE_ITEM, handler: this.receiveMoveItemTo.bind(this) },

            { event: GamePlay.events.RECEIVE_PLAYER_JOIN, handler: this.receivePlayJoin.bind(this) },
            { event: GamePlay.events.RECEIVE_PLAYER_LEAVE, handler: this.requestBoardInfo.bind(this) },
            { event: GamePlay.events.RECEIVE_GAME_OVER, handler: this.receiveGameOver.bind(this) },
            { event: GamePlay.events.RECEIVE_DRAW_OFFER, handler: this.receiveDrawOffer.bind(this) },
            { event: GamePlay.events.REPLY_LEAVE_GAME, handler: this.receiveLeaveGame.bind(this) },
        ];
        this.connection = cm.createConnection(GamePlay.url, listener, this.requestBoardInfo.bind(this));
    },
    showNotify(text, time) {
        if (!this.nodeNotify)
            return;
        this.nodeNotify.getChildByName("text").getComponent(cc.Label).string = text;
        this.nodeNotify.active = true;
    },
    hideNotify() {
        if (!this.nodeNotify)
            return;
        this.nodeNotify.getChildByName("text").getComponent(cc.Label).string = "";
        this.nodeNotify.active = false;
    },
    updateUI() {
        let boardInfo = this.boardInfo;
        switch (boardInfo.status) {
            case "New":
                this.showNotify("Đang chờ đối thủ ....");
                break;
            case "Ready":
                this.showNotify("Sẳn sàng thi đấu ...");
                break;
            case "StartGame":
                this.hideNotify();
                break;
            case "Playing":
                this.hideNotify();
                break;
            default:
                break;
        }
        this.leftMenu.refreshMenu(boardInfo);
    },
    showMessage(message) {
        alert(message);
    },
    readyToRequest() {
        return (
            this.requestGamePlay &&
            this.requestGamePlay.boardId &&
            this.requestGamePlay.playerId
        );
    },
    requestBoardInfo() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_BOARD, request);
        }
    },
    receiveBoardInfo(response) {
        if (response.ok) {
            this.applyBoardInfo(response.data);
        }
        else {
            this.showMessage(response.message);
        }
    },
    receivePlayJoin(response) {
        if (response.ok) {
            this.applyBoardInfo(response.data);
        }
        else {
            this.showMessage(response.message);
        }
    },
    applyBoardInfo(data) {
        if (data.redPlayer && data.redPlayer.id == this.ownerId) {
            this.mySide = ChessDefinition.COLOR.RED;
            this.firstPlayer.setPlayerInfo(data.redPlayer);
            this.secondPlayer.setPlayerInfo(data.blackPlayer);
            this.board.setDimension(ChessDefinition.COLOR.RED);
        }
        else
            if (data.blackPlayer && data.blackPlayer.id == this.ownerId) {
                this.mySide = ChessDefinition.COLOR.BLACK;
                this.firstPlayer.setPlayerInfo(data.blackPlayer);
                this.secondPlayer.setPlayerInfo(data.redPlayer);
                this.board.setDimension(ChessDefinition.COLOR.BLACK);
            }
            else {
                this.mySide = "";
                this.firstPlayer.setPlayerInfo(data.redPlayer);
                this.secondPlayer.setPlayerInfo(data.blackPlayer);
                this.board.setDimension(ChessDefinition.COLOR.RED);
            }
        this.boardInfo = data;
        this.board.setInfo(data);
        this.updateUI();
    },
    receiveResult(response) {
        if (!response.ok) {
            this.showMessage(response.message);
        }
        else {
            this.requestBoardInfo();
        }
    },
    receiveStartGame(response) {
        if (response.ok) {
            let firstPlayer = this.boardInfo.redPlayer.name;
            let secondPlayer = this.boardInfo.blackPlayer.name;
            this.board.reset();
            this.board.setInfo(response.data);
            this.startGame.startGame(firstPlayer, secondPlayer);
        }
    },
    buttonSwitchSideOnClick() {
        if (this.isRed()) {
            let { requestGamePlay } = this;
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_SWITCH_SIDE, requestGamePlay);
        }
    },
    buttonReadyOnClick() {
        let { requestGamePlay } = this;
        cm.sendRequest(this.connection, GamePlay.commands.REQUEST_READY_TO_PLAY, requestGamePlay);
    },
    buttonResignOnClick() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_RESIGN, request);
        }
    },
    receiveGameOver(response) {
        if (response.ok) {
            let { data } = response;
            let { result, description, redPlayer, blackPlayer } = data;
            this.gameOver.show(result, description, redPlayer, blackPlayer,
                () => {
                    this.board.reset();
                    if (this.firstPlayer)
                        this.firstPlayer.resetTimer();
                    if (this.secondPlayer)
                        this.secondPlayer.resetTimer();
                    this.requestBoardInfo();
                });
        }
    },
    buttonDrawOfferOnClick() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_DRAW, request);
        }
    },
    receiveDrawOffer(response) {
        if (response.ok)
            this.drawOffer.node.active = true;
    },
    buttonAcceptDrawOnClick() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay, accept: true };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_ACCEPT_DRAW, request);
        }
        this.drawOffer.node.active = false;
    },
    buttonCancelDrawOnClick() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay, accept: false };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_ACCEPT_DRAW, request);
        }
        this.drawOffer.node.active = false;
    },
    isRed() {
        let { boardInfo, ownerId } = this;
        return (boardInfo && boardInfo.redPlayer && boardInfo.redPlayer.id == ownerId)
    },
    requestPickItem(item) {
        let { requestGamePlay } = this;
        let request = {
            ...requestGamePlay,
            type: item.type,
            color: item.color,
            row: item.row,
            col: item.col
        };
        cm.sendRequest(this.connection, GamePlay.commands.REQUEST_PICK_ITEM, request);
    },
    receivePickItem(response) {
        if (response.ok) {
            let { data } = response;
            this.board.selectItem(data);
        }
        else {
            this.requestBoardSnapshot();
        }
    },
    requestMoveItemToCell(item, cell) {
        let { requestGamePlay } = this;
        let request = {
            ...requestGamePlay,
            fromType: item.type,
            fromColor: item.color,
            fromRow: item.row,
            fromCol: item.col,
            toType: '',
            toColor: '',
            toRow: cell.row,
            toCol: cell.col,

        };
        cm.sendRequest(this.connection, GamePlay.commands.REQUEST_MOVE_ITEM, request);
    },
    requestMoveItemToKill(item, target) {
        let { requestGamePlay } = this;
        let cell = target.cell;
        let request = {
            ...requestGamePlay,
            fromType: item.type,
            fromColor: item.color,
            fromRow: item.row,
            fromCol: item.col,
            toType: target.type,
            toColor: target.color,
            toRow: cell.row,
            toCol: cell.col
        };
        cm.sendRequest(this.connection, GamePlay.commands.REQUEST_MOVE_ITEM, request);
    },
    receiveMoveItemTo(response) {
        if (response.ok) {
            let { data } = response;
            this.board.moveItem(data.item, data.destination);
            this.board.killItem(data.kill);
            if (data.redPlayer && data.redPlayer.id == this.ownerId) {
                this.firstPlayer.setTimeInfo(data.redPlayer);
                this.secondPlayer.setTimeInfo(data.blackPlayer);
            }
            else
                if (data.blackPlayer && data.blackPlayer.id == this.ownerId) {
                    this.firstPlayer.setTimeInfo(data.blackPlayer);
                    this.secondPlayer.setTimeInfo(data.redPlayer);
                }
                else {
                    this.firstPlayer.setTimeInfo(data.redPlayer);
                    this.secondPlayer.setTimeInfo(data.blackPlayer);
                }
            if (data.warningMessage && (data.warningSide == this.mySide)) {
                this.notify.showMessage(data.warningMessage, 2);
            }
            else
                if (!data.isGameOver && data.isCheckMate) {
                    this.checkMate.flash(1);
                }
            //debug
            let debugInfo = {
                redPlayer: {
                    isYourTurn: data.redPlayer.isYourTurn,
                    remainTime: data.redPlayer.remainTime,
                    remainMoveTime: data.redPlayer.remainMoveTime,
                },
                blackPlayer: {
                    isYourTurn: data.blackPlayer.isYourTurn,
                    remainTime: data.blackPlayer.remainTime,
                    remainMoveTime: data.blackPlayer.remainMoveTime,
                }
            }
            //this.debug(debugInfo);
        }
        else {
            //alert(response.message);
            //this.requestBoardSnapshot();
        }
    },
    requestBoardSnapshot() {
        let { requestGamePlay } = this;
        let request = { ...requestGamePlay };
        cm.sendRequest(this.connection, GamePlay.commands.REQUEST_BOARD_SNAPSHOT, request);
    },
    receiveBoardSnapshot(response) {
        if (response.ok) {
            let { data } = response;
            this.boardInfo.turn = data.turn;
            this.boardInfo.items = data.items;
            this.board.setInfo(data);;
        }
    },
    start() {
    },
    buttonLeaveGameOnClick() {
        if (this.readyToRequest()) {
            let request = { ...this.requestGamePlay };
            cm.sendRequest(this.connection, GamePlay.commands.REQUEST_LEAVE_GAME, request);
        }
    },
    receiveLeaveGame(response) {
        if (response.ok) {
            Global.board = null;
            cc.director.loadScene('Room');
        }
    },
});
