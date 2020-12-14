
let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');
var LoginForm = cc.Class({
    extends: cc.Component,
    statics: {
        current: null,
        connection: null,
        url: GLOBAL.Host + '/hubs/login',
        commands: {
            REQUEST_LOGIN: 'RequestLogin',
        },        
    },

    properties: {
        username: cc.EditBox,
        password: cc.EditBox,
        message:cc.Node,
    },    
    start() {
        LoginForm.current = this;
        this.initConnection();      
        this.message = this.message.getComponent("Message");  
    },
    initConnection(){
        let listener = [
            {   event: 'ReceiveLogin',handler: this.receiveLogin.bind(this)},
            {   event: 'ReceiveDisconnectCommand', handler: this.receiveDisconnectCommand.bind(this)},
        ];
        if (!LoginForm.connection || LoginForm.connection.start)
            LoginForm.connection = cm.createConnection(LoginForm.url, listener,this.ready());
    },
    receiveDisconnectCommand(response){
        alert("Tài khoản của bạn vừa được đăng nhập ở một máy khác");
        Global.gameManager.forceLogin();
    },
    ready(){
        this.node.active = true;
    },
    receiveLogin(response){
        if (response.ok) {
            Global.account = response.data;
            Global.player = response.data.player;
            cc.director.loadScene('Room');
        }
        else {
            this.message.show(response.message);
        }
    },
    btnLoginOnClick() {
        // get user/password
        let request = {
            user: this.username.string,
            password: this.password.string,
        };
        if (!request.user || request.user === '') {
            this.message.show("Vui lòng nhập tài khoản");
        }
        else
            if (!request.password || request.password === '') {
                this.message.show("Vui lòng nhập  mật khẩu");
            }
            else
                cm.sendRequest(LoginForm.connection, LoginForm.commands.REQUEST_LOGIN, request,null,(e)=>{
                    this.message.show(e.message);
                });
    }, 
});
