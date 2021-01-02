using Microsoft.AspNetCore.Identity;
using MongoDB.Bson.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Models
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public Guid PlayerId { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime? LastLogout { get; set; }
    }
    
    public class PlayerInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string RankLabel { get; set; }
        public int RankIndex { get; set; }
        public int StarIndex { get; set; }
    }
    public class Player
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Coin { get; set; }
        public string RankLabel { get; set; }
        public int RankIndex { get; set; }
        public int StarIndex { get; set; }
        public int TotalGame { get; set; }
        public int TotalWin { get; set; }
        public int TotalDraw { get; set; }
        public int TotalLose { get; set; }      
        public void AddWin(int score,int betCoin)
        {
            TotalGame += 1;
            TotalWin += 1;
            Score += score;
            Coin += (int)Math.Floor((decimal)betCoin * 9 / 10);
        }
        public void AddLose(int score, int betCoin)
        {
            TotalGame += 1;
            TotalLose += 1;
            Score -= score;
            Coin -= (int)Math.Floor((decimal)betCoin*9/10);
        }
        public void AddDraw()
        {
            TotalGame += 1;
            TotalDraw += 1;
        }
        public void AddScore(int score)
        {
            Score += score;
        }
    }
}
