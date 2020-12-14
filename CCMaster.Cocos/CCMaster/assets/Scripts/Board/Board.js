let ChessDefinition = require("ChessDefinition");
var Board = cc.Class({
    extends: cc.Component,
    statics: {
        ROWS: 10,
        COLS: 9
    },
    properties: {
        nodeCell: cc.Node,
        nodeItem: cc.Node,
        nodeItemType: cc.Node,
        nodeGamePlay: cc.Node,
    },
    onLoad() {
        if (this.nodeGamePlay)
            this.gamePlay = this.nodeGamePlay.getComponent("GamePlay");
        this.items = [];
        this.initCells();
        this.turn = '';
        this.lastHints =[];
    },
    setDimension(color) {
        if (color == ChessDefinition.COLOR.BLACK)
            this.dimension = ChessDefinition.COLOR.BLACK;
        else
            this.dimension = ChessDefinition.COLOR.RED;
    },
    reload() {
        this.clearCells();
        let selectCell = this.selectCell.bind(this);
        this.items.forEach(item => {
            let cell = selectCell(item);
            if (cell)
                item.setCell(cell);
        });
    },
    clearCells() {
        if (!this.cells || this.cells.length === 0)
            this.initCells();
        else
            this.cells.forEach(cell => {
                cell.setActive(false);
            });
    },
    clearItems() {
        this.items.forEach(item => {
            item.setCell(null);
            item.setLocation(0, 0);
        });
    },
    clear() {
        this.clearCells();
        this.clearItems();
    },
    reset() {
        this.items.forEach(item => {
            item.deselect();
            item.unaimed();
        });     
    },
    setInfo(info) {
        if (this.items.length == 0)
            this.initItems();
        else {
            this.items.forEach(item => {
                item.setCell(null);
                item.setLocation(0, 0);
            });
        }
        this.turn = info.turn;
        let { items } = info;
        if (!items)
            items = [];
        let pickItem = this.pickItem.bind(this);
        items.forEach(info => {
            let item = pickItem(info.color, info.type);
            if (item) {
                item.setLocation(info.row, info.col);
            }
        });
        this.reload();
    },
    pickItem(color, type) {
        return this.items.find(item => item.color == color && item.type == type && !item.isAlive());
    },
    getItem(itemInfo) {
        return this.items.find(item =>
            item.type == itemInfo.type &&
            item.color == itemInfo.color &&
            item.row == itemInfo.row &&
            item.col == itemInfo.col
        );
    },
    getCell(row, col) {
        return this.cells.find(cell => cell.row == row && cell.col == col);
    },
    selectCell(info) {
        return (this.dimension == info.color ? this.getCell(info.row, info.col)
            : this.getCell(Board.ROWS - info.row + 1, Board.COLS - info.col + 1));
    },
    //====================================================================================
    initCells() {
        this.cells = [];
        for (let r = Board.ROWS; r >= 1; r--) {
            for (let c = Board.COLS; c >= 1; c--) {
                let cellNode = cc.instantiate(this.nodeCell);
                this.node.addChild(cellNode);
                let cell = cellNode.getComponent("Cell");
                cell.setBoard(this);
                cell.setLocation(r, c);
                this.cells.push(cell);
            }
        }
    },
    initItems() {
        this.items = [];
        let colors = [ChessDefinition.COLOR.RED, ChessDefinition.COLOR.BLACK];
        for (let c = 0; c < colors.length; c++) {
            let color = colors[c];
            this.createItem(color, ChessDefinition.TYPE.KING);
            for (let i = 1; i <= 2; i++) {
                this.createItem(color, ChessDefinition.TYPE.ADVISOR);
                this.createItem(color, ChessDefinition.TYPE.ELEPHANT);
                this.createItem(color, ChessDefinition.TYPE.CHARIOT);
                this.createItem(color, ChessDefinition.TYPE.CANON);
                this.createItem(color, ChessDefinition.TYPE.HORSE);
            };
            for (let i = 1; i <= 5; i++) {
                this.createItem(color, ChessDefinition.TYPE.PAWN);
            }
        };
    },
    createItem(color, type) {
        let node = cc.instantiate(this.nodeItem);
        this.node.addChild(node);
        let nodeType = this.nodeItemType.getChildByName(color+"-"+type);
        node.getChildByName("avatar").getComponent(cc.Sprite).spriteFrame = nodeType.getComponent(cc.Sprite).spriteFrame;

        let item = node.getComponent("Item");

        item.zeroPosition = this.nodeItem.getPosition();
        item.type = type;
        item.color = color;

        node.on(cc.Node.EventType.MOUSE_UP, this.onItemClick.bind(this,item));
        node.on(cc.Node.EventType.MOUSE_ENTER, this.onItemMouseEnter.bind(this,item));
        node.on(cc.Node.EventType.MOUSE_LEAVE, this.onItemMouseLeave.bind(this,item));

        item.reset();
        this.items.push(item);
    },
    onItemMouseEnter(item) {
        if (this.canKill(item))
            item.aimed();
    },
    onItemMouseLeave(item) {
        if (item.isAimed)
            item.unaimed();
    },
    onItemClick(item) {
        if (this.selectedItem) {
            if (item.color == this.dimension) {
                this.selectedItem.deselect();
                this.gamePlay.requestPickItem(item);
            }
            else {
                this.gamePlay.requestMoveItemToKill(this.selectedItem, item);
            }
        }
        else
            this.gamePlay.requestPickItem(item);
    },
    selectItem(info) {
        let item = this.getItem(info);
        if (item) {
            if (this.selectedItem) {
                this.selectedItem.deselect();
                this.hideHint();
            }
            item.scopes = info.scopes;
            this.selectedItem = item;
            this.selectedItem.select();
            this.showHint(true, item.scopes);
        }
    },
    moveItem(from, to) {
        let item = this.getItem(from);
        if (!item) {
            alert("Can not find item at (" + from.row + "," + from.col + ")");
            return;
        }
        let cell = this.selectCell({ color: item.color, row: to.row, col: to.col });
        if (item && cell) {
            item.moveTo(cell.node.x, cell.node.y);
            item.setCell(cell);
            item.setLocation(to.row, to.col);
            this.hideHint();
            this.selectedItem = null;
        }
    },
    killItem(info) {
        if (!info)
            return;
        let item = this.getItem(info);
        if (item) {
            item.killed();
        }
    },
    onCellClick(cell) {
        if (this.selectedItem) {
            this.gamePlay.requestMoveItemToCell(this.selectedItem, cell);
        }
    },
    onCellEnter(cell) {
        if (this.canMoveTo(cell)) {
            cell.setActive(true);
        }
    },
    onCellLeave(cell) {
        if (cell.active)
            cell.setActive(false);
    },
    canKill(item) {
        if (!this.selectedItem)
            return false;
        if (this.selectedItem.color == item.color)
            return false;
        let cell = item.cell;
        if (cell) {
            let hint = this.lastHints.find(i =>
                i.row == cell.row &&
                i.col == cell.col);
            return (!!hint)
        }
        else
            return false;
    },
    canMoveTo(cell) {
        if (!this.selectedItem)
            return false;
        let hint = this.lastHints.find(i =>
            i.row == cell.row &&
            i.col == cell.col);
        return (!!hint)
    },
    showHint(active, scopes) {
        if (scopes && scopes.length > 0) {
            this.lastHints = scopes;
            this.lastHints.forEach(scope => {
                let cell = this.cells.find(c => c.row == scope.row && c.col == scope.col);
                if (cell) {
                    let item = cell.item;
                    if (item && item.isAlive())
                        item.showHint(active);
                    else
                        cell.showHint(active);
                }
            })
        }
    },
    hideHint() {
        this.cells.forEach(cell => {
            if (cell.hint) cell.showHint(false)
        });
        this.items.forEach(item => {
            item.showHint(false);
        });
    },
});
