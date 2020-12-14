const Status = {
    START: "start",
    STOP: "stop",
}
var Clock = cc.Class({
    extends: cc.Component,

    properties: {
        nodeSeconds: cc.Node,
        radius: 100,
    },
    onLoad() {
        this.startValue = 0;
        this.currentValue = 0;
        this.status = Status.STOP;
        this.node.active = false;

        this.ctx = this.node.getComponent(cc.Graphics);
        this.x = 0;
        this.y = 0;
        this.label = this.nodeSeconds.getComponent(cc.Label);
        this.scheduleSelector = this.countDown.bind(this);
    },
    startClock(value) {
        this.resetValue = this.startValue = this.currentValue = value || 0;
        this.draw();
        this.startTime = new Date();
        this.status = Status.START;
        this.schedule(this.scheduleSelector, 1);
    },
    reset(){
        this.startValue = this.currentTime = this.resetValue;
        this.stopClock();        
    },
    countDown() {
        if (this.status == Status.STOP)
            return;
        let currentTime = new Date();
        let seconds = Math.round((currentTime.getTime() - this.startTime.getTime()) / 1000);
        this.currentValue = this.startValue - seconds;
        if (this.currentValue < 0)
            this.currentValue = 0;
        this.draw();
    },
    stopClock() {
        this.status = Status.STOP;
        this.unschedule(this.scheduleSelector);
        this.node.active = false;
    },
    draw() {
        let text = (this.currentValue < 10 ? "0" : "") + this.currentValue;
        this.label.string = text;
        this.ctx.clear();
        this.ctx.moveTo(this.x, this.y);
        this.ctx.lineTo(this.x, this.y + this.radius)
        this.ctx.arc(this.x, this.y, this.radius, Math.PI / 2, Math.PI / 2 - 2 * Math.PI * (this.startValue - this.currentValue) / this.startValue);
        this.ctx.lineTo(this.x, this.y)
        this.ctx.fill();
        if (!this.node.active)
            this.node.active = true;
    },
});
