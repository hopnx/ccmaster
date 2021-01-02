let Board = require("Board");
cc.Class({
    extends: cc.Component,
    properties: {
    },
    onLoad() {
        //register User Input Event
        this.node.on(cc.Node.EventType.MOUSE_UP, this.onCellClick.bind(this));
        this.node.on(cc.Node.EventType.MOUSE_ENTER, this.onCellEnter.bind(this));
        this.node.on(cc.Node.EventType.MOUSE_LEAVE, this.onCellLeave.bind(this));

        this.row = 0;
        this.col = 0;
        this.label = '';
        this.active = false;
        this.hint = false;
        this.board = null;

        this.width = this.node.width;
        this.height = this.node.height;
    },
    inversedRow(){
        return Board.ROWS - this.row||0 + 1;
    },
    inversedCol(){
        return Board.COLS - this.col||0 + 1;
    },
    getInfo() {
        return {
            label: this.label,
            row: this.row,
            col: this.col,
            active: this.active,
            selected: this.selected,
        }
    },
    onCellClick() {
        if (this.board)
            this.board.onCellClick(this);
    },
    onCellEnter() {
        if (this.board) {
            this.board.onCellEnter(this);
        }
    },
    onCellLeave() {
        if (this.board) {
            this.board.onCellLeave(this);
        }
    },
    //
    setActive(active) {
        this.active = active;
        this.node.getChildByName('active').active = this.active;
    },
    setBoard(board) {
        this.board = board;
    },
    getWidth() {
        return this.node.width;
    },
    getHeight() {
        return this.node.height;
    },
    showHint(show) {
        let activeNode = this.node.getChildByName("hint");
        if (activeNode) {
            this.hint = !!show;
            activeNode.active = this.hint;
        }
    },
    setLabel(label) {
        this.label = label;
    },
    setPosition(cell) {
        if (cell) {
            let position = cell.getPosition();
            this.node.setPosition(position.x, position.y);
            this.cell = cell;
        }
    },
    getPosition() {
        return this.node.position;
    },
    setLocation(row, col) {
        this.row = row;
        this.col = col;
        let x = ((Board.COLS + 1) / 2 - col) * this.width;
        let y = (row - (Board.ROWS + 1) / 2) * this.height;
        this.node.setPosition(x,y);
        this.label = "["+row+","+col+"]";
    }
});
