var Flash = cc.Class({
    extends: cc.Component,

    properties: {
    },
    onLoad(){
        //this.node.active = false;
    },
    start () {
    }, 
    startGame(firstPlayer,secondPlayer){
        let first = this.node.getChildByName("first");
        let second = this.node.getChildByName("second");
        if (first)
            first.getComponent(cc.Label).string = firstPlayer;
        if (second)
            second.getComponent(cc.Label).string = secondPlayer;
        this.node.active = true;
        this.scheduleOnce(()=>{
            this.node.active = false;
        },1);
    },
    flash(seconds){
        this.node.active = true;
        this.scheduleOnce(()=>{
            this.node.active = false;
        },seconds||1)
    },    
    showMessage(message,seconds){
        this.node.getChildByName("text").getComponent(cc.Label).string = message;
        this.node.active = true;
        this.scheduleOnce(()=>{
            this.node.active = false;
        },seconds||1)
    },

});
