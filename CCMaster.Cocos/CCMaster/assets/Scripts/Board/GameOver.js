var GameOver = cc.Class({
    extends: cc.Component,

    properties: {
    },
    onLoad() {
    },
    start() {
    },
    show(result,description, redPlayer, blackplayer, callback) {
        cc.find("title/red_win", this.node).active = (result == "RED_WIN");
        cc.find("title/black_win", this.node).active = (result == "BLACK_WIN");
        cc.find("title/draw", this.node).active = (result == "DRAW");
        cc.find("title/description", this.node).getComponent(cc.Label).string = description;
       
        cc.find("redPlayer/icon/d_win", this.node).active = (result == "RED_WIN");
        cc.find("redPlayer/icon/d_lose", this.node).active = (result == "BLACK_WIN");
        cc.find("redPlayer/icon/d_draw", this.node).active = (result == "DRAW");
        cc.find("redPlayer/name", this.node).getComponent(cc.Label).string = redPlayer.name;
        cc.find("redPlayer/score", this.node).getComponent(cc.Label).string = redPlayer.score;

        cc.find("blackPlayer/icon/d_win", this.node).active = (result == "BLACK_WIN");
        cc.find("blackPlayer/icon/d_lose", this.node).active = (result == "RED_WIN");
        cc.find("blackPlayer/icon/d_draw", this.node).active = (result == "DRAW");
        cc.find("blackPlayer/name", this.node).getComponent(cc.Label).string = blackplayer.name;
        cc.find("blackPlayer/score", this.node).getComponent(cc.Label).string = blackplayer.score;

        this.node.active = true;
        this.scheduleOnce(() => {
            this.node.active = false;
            if (callback)
                callback();
        }, 3);
    },

});
