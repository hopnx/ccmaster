module.exports = {
    COLOR:{
        RED:"red",
        BLACK:"black"
    },
    TYPE:{
        KING: "king",
        ADVISOR:"advisor",
        ELEPHANT:"elephant",
        CHARIOT:"chariot",
        CANON:"canon",
        HORSE:"horse",
        PAWN:"pawn"
    },
    RANK:{
        "0":"Gà non",
        "1":"Tập sự",
        "2":"Kỳ thủ",
        "3":"Đại sư",
    },    
    getRankText(rank){
        let text = this.RANK[rank+""];
        return text || "";
    }
}