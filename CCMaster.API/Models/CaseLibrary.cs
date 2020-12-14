using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Models
{
    public class BoardCase
    {
        static public void DonMaThang(Board board)
        {
            board.ClearBoard();
            board.SetItem(Color.RED, ItemType.KING, 1, 5);
            board.SetItem(Color.RED, ItemType.HORSE, 6, 5);

            board.SetItem(Color.BLACK, ItemType.KING, 1, 4);
        }
        static public void DonXeThang(Board board)
        {
            board.ClearBoard();
            board.SetItem(Color.RED, ItemType.KING, 1, 5);
            board.SetItem(Color.RED, ItemType.CHARIOT, 6, 5);

            board.SetItem(Color.BLACK, ItemType.KING, 1, 4);
        }
    }
}
