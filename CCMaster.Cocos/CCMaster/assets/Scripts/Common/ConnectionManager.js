import { HubConnectionBuilder } from '@microsoft/signalr';
const ConnectionManager = {
    version: "1.0",
    author:"ng.xuan.hop@gmail.com",
    createConnection: function (url, listener,callback) {
        let connection = new HubConnectionBuilder()
            .withUrl(url)
            .withAutomaticReconnect()
            .build();
        connection.registeredListener =  listener||[];
        connection.registeredCallback = callback;
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('Connected!');
                    let list = connection.registeredListener;
                    list.map(item=>{
                        connection.on(item.event,item.handler);
                    });
                    if (connection.registeredCallback){
                        connection.registeredCallback();
                    }
                })
                .catch(e => console.log('Connection failed: ', e));
        }
        return connection;
    },
    sendRequest: function(connection,command,param,onSuccess,onError){
        if (connection.connectionStarted) {
            try {
                connection.send(command,param);
                if (onSuccess)
                    onSuccess();
            }
            catch (e) {
                if (onError)
                    onError(e);
            }
        }
        else {     
            if (onError)
                onError({message:"Kết nối đến server chưa thành công"});       
            //('No connection to server yet.');
            this.reconnect(connection);
            
        }
    },
    reconnect(connection){
        if (connection) {
            connection.start()
                .then(() => {
                    console.log('Connected!');
                    let list = connection.registeredListener;
                    list.map(item=>{
                        connection.on(item.event,item.handler);
                    });
                    if (connection.registeredCallback){
                        connection.registeredCallback();
                    }
                })
                .catch(e => console.log('Connection failed: ', e));
        }       
    }
}

module.exports = ConnectionManager;