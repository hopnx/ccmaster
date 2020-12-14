const GlobalSettings = require("./GlobalSetting");

cc.Class({
    extends: cc.Component,
    properties: {

    },
   onLoad () {
   },
    onStart() {
    },
    show(message) {
        let label = this.node.getChildByName("text");
        if (label)
            label.getComponent(cc.Label).string = message;
        this.node.active = true;
        this.node.on(cc.Node.EventType.MOUSE_UP, this.hide.bind(this));   
    },
    hide() {
        this.node.active = false;
    }
    // update (dt) {},
});
