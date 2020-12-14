let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');

var Room = cc.Class({
    extends: cc.Component,
    statics: {
        current:null,
        url: GLOBAL.Host + '/hubs/board',
        commands: {
            REQUEST_PLAY_GAME: 'RequestPlayGame',
        }        
    },
    properties: {

    },
    // LIFE-CYCLE CALLBACKS:
    onLoad () {
        this.player = cc.find("LeftPanel/Player",this.node).getComponent("Player");
        this.player.playerId = Global.account.player.id;
        let listener = [
            {
                event: 'ReceivePlayGame',
                handler: (response) => {
                    if (response.ok) {
                        Global.board = response.data;
                        cc.director.loadScene('Board');
                    }
                    else {
                        this.showMessage(response.message);
                    }
                }
            }
        ];
        this.connection = cm.createConnection(Room.url, listener);
    },
    start () {
    },
    onStartClick(){
        let request = {
            playerId: this.player.playerId
        }
        cm.sendRequest(this.connection, Room.commands.REQUEST_PLAY_GAME, request);
    },
    showMessage(message){
        alert(message);
    }
});
