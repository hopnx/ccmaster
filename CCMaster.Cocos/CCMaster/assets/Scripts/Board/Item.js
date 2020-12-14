var Item = cc.Class({
    extends: cc.Component,
    properties: {
        itemType: cc.Node,
    },
    onLoad() {
        //init item info
        this.type = '';
        this.color = '';
        this.row = 0;
        this.col = 0;
        this.scopes = []; // lưu trữ vị trí các nước có thể đi của quân cờ
        this.selected = false;
        this.isAimed = false;
        this.cell = null;
        this.label = '';
    },
    getInfo() {
        return {
            type: this.type,
            color: this.color,
            row: this.row,
            col: this.col,
            selected: this.selected,
            scopes: this.scopes,
            label: this.label,
        }
    },
    setItemType(color, type) {
        let itemType = this.itemType.getComponent("Type");
        let sprite = itemType.getSprite(color, type);
        let avatar = this.node.getChildByName("avatar");
        if (avatar && sprite) {
            avatar.getComponent(cc.Sprite).spriteFrame = sprite.spriteFrame;
            this.color = color;
            this.type = type;
        }
    },
    setLocation(row, col) {
        this.row = row || 0;
        this.col = col || 0;     
    },
    isAlive() {
        return (this.row > 0 && this.col > 0);
    },
    setLabel(label) {
        this.label = label;
    },
    setCell(cell) {
        let old_cell = this.cell;
        if (old_cell)
            old_cell.item = null;

        if (cell) {
            this.cell = cell;
            cell.item = this;
            this.setPosition(cell.getPosition());
            this.setLabel(cell.label);
            this.node.active = true;
        }
        else {
            this.cell = null;
            this.row = 0;
            this.col = 0;
            this.scopes = []; // lưu trữ vị trí các nước có thể đi của quân cờ
            this.selected = false;
            this.isAimed = false;
            this.label = '';
            this.node.active = false;
        }
    },
    setPosition(position) {
        this.node.setPosition(position.x, position.y);
    },
    //=============================================================================================
    reset(){
        this.setLocation(0, 0);
        this.deselect();
        this.unaimed();
        this.setPosition(this.zeroPosition);
        this.setCell(null);
    },
    //============================================================================================
    select() {
        this.selected = true;
        this.node.getChildByName("selected").active = this.selected;
    },
    deselect() {
        this.selected = false;
        this.node.getChildByName("selected").active = this.selected;
    },
    aimed() {
        this.isAimed = true;
        this.node.getChildByName("aimed").active = this.isAimed;
    },
    unaimed() {
        this.isAimed = false;
        this.node.getChildByName("aimed").active = this.isAimed;
    },

    moveTo(x, y) {
        cc.tween(this.node)
            .to(0.2, { position: cc.v2(x,y), angle: 0 })
            .start();
    },
    killed() {
      this.reset();
    },
    showHint(active) {
        let activeNode = this.node.getChildByName("active-item");
        if (activeNode) {
            this.active = !!active;
            activeNode.active = this.active;
        }
    },

});
