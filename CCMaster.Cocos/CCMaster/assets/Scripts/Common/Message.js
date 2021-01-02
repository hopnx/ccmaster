var MessageManager = cc.Class({
    extends: cc.Component,
    properties: {
    },
    onLoad () {
        if (this.node.active)
            this.node.active = false;
        this.node.on(cc.Node.EventType.MOUSE_UP,this.hide.bind(this));
        this.node.on(cc.Node.EventType.TOUCH_START,this.hide.bind(this));
    },
    show(message,seconds,callback){
        this.labelNode = this.node.getChildByName("Label");
        if (this.labelNode){
            this.label = this.labelNode.getComponent(cc.Label);      
        }
        if (this.label)
            this.label.string = message;
        this.node.active = true;
        this.scheduleOnce(this.hide.bind(this,callback),seconds||2);
    },
    hide(callback){
        this.node.active = false;
        if (this.label)
            this.label.string = "";
        if (callback)
            callback();
    },
});
