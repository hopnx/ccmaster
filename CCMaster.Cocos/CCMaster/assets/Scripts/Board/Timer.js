const Status = {
    START: "start",
    STOP: "stop",
    IDLE: "idle",
}
var Timer = cc.Class({
    extends: cc.Component,
    properties: {      
    },
    onLoad() {
        this.startValue = 0;
        this.currentValue = 0;
        this.status = Status.STOP;

        this.label = this.node.getChildByName("label").getComponent(cc.Label);
        this.scheduleSelector = this.countDown.bind(this);
    },
    setStartValue(value){
        this.resetValue = this.startValue = this.currentValue = value || 0;
        this.draw();
    },
    reset(){
        this.stopTimer();
        this.startValue = this.currentValue = this.resetValue;
    },
    startTimer(value) {
        this.startValue = Math.round(value || 0);
        this.currentValue = this.startValue;
        this.draw();
        this.startTime = new Date();
        this.status = Status.START;
        this.schedule(this.scheduleSelector,1);
    },
    countDown() {      
        if (this.status == Status.STOP)
        return; 
        let currentTime = new Date();
        let seconds = Math.round((currentTime.getTime() - this.startTime.getTime()) / 1000);
        this.currentValue = this.startValue - seconds;
        if (this.currentValue<0)
            this.currentValue = 0;
        this.draw();
    },
    stopTimer() {
        this.status = Status.STOP;
        this.unschedule(this.scheduleSelector);
   },
    draw(){
        let seconds = this.currentValue % 60;
        let minutes = Math.round((this.currentValue - seconds) / 60);
        let labelSeconds = (seconds < 10 ? "0" : "") + seconds;
        let labelMinutes = (minutes < 10 ? "0" : "") + minutes;
        this.label.string = labelMinutes+":"+labelSeconds;
        if (!this.node.active)
        this.node.active=true;
    },
});
