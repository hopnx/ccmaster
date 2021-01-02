
let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');
var LoginForm = cc.Class({
    extends: cc.Component,
    statics: {
        commands: {
            REQUEST_LOGIN: 'RequestLogin',
        },     
        connection: null,
    },
    properties: {
        username: cc.EditBox,
        password: cc.EditBox,
    },    
    onLoad(){
        this.node.active = false;
        // init connection
        let listener = [
            {   event: 'ReceiveLogin',handler: this.receiveLogin.bind(this)},
            {event: 'ReceiveDisconnectCommand', handler: this.receiveDisconnectCommand.bind(this)},   
        ];
        LoginForm.connection = cm.createConnection(GLOBAL.Host+'/hubs/login',listener,this.readyToLogin.bind(this));
    },    
    readyToLogin(){
        this.node.active = true;
    },
    receiveLogin(response){
        if (response.ok) {
            Global.accountId = response.data.id;
            Global.playerId = response.data.playerId;
            Global.gameManager.openHomeScene();
        }
        else {
            Global.gameManager.showMessage(response.message);
        }
    },
    receiveDisconnectCommand(){
        Global.gameManager.showMessage("Tài khoản của bạn vừa được đăng nhập ở một máy khác",Global.gameManager.forceLogin);
    },
    showMessage(message){
        Global.gameManager.showMessage(message);
    },
    btnLoginOnClick() {
        // get user/password
        let request = {
            user: this.username.string,
            password: this.password.string,
        };
        if (!request.user || request.user === '') {
            this.showMessage("Vui lòng nhập tài khoản");
        }
        else
            if (!request.password || request.password === '') {
                this.showMessage("Vui lòng nhập  mật khẩu");
            }
            else
                cm.sendRequest(LoginForm.connection, LoginForm.commands.REQUEST_LOGIN, request,null,(e)=>{
                    this.showMessage(e.message);
                });
    }, 
});
