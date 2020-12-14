window.Global = {
}
cc.Class({
    extends: cc.Component,    
    properties: {        
        message: cc.Node
    },
    start () {
       Global.gameManager = this;
    },
    forceLogin(){
        this.account = null;        
        cc.director.loadScene('Login');
    }
});
