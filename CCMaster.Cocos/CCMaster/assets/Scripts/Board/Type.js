let ChessDefinition = require("ChessDefinition");
cc.Class({
    extends: cc.Component,
    properties: {
    },
    getSprite(color,type){
      let name = color+"-"+type;
      let child = this.node.getChildByName(name);
      if (child){
        return child.getComponent(cc.Sprite);
      }
    }
});
