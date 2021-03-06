﻿using CCMaster.API.Models;
using CoreLibrary.Base;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace CCMaster.API.Domains
{
    public class DOGame
    {
        public Guid Id { get; set; }
        public DOGameSide RedPlayer { get; set; }
        public DOGameSide BlackPlayer { get; set; }
        public List<DOItem> Items { get; set; }
        public string Status { get; set; }
        public string Turn { get; set; }
        public string WarningSide { get; set; }
        public string WarningMessage { get; set; }
    }
    public class DOGamePlayer
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public string Turn { get; set; }
        public DOGameSide RedPlayer { get; set; }
        public DOGameSide BlackPlayer { get; set;}
        public string WarningSide { get; set; }
        public string WarningMessage { get; set; }
    }    
    public class DOGameSide
    {
        public Guid? Id { get; set; }
        /*
        public string Name { get; set; }
        public string RankLabel { get; set; }
        public int? RankIndex { get; set; }
        public int? StarIndex { get; set; }
        */
        public string Status { get; set; }
        public bool IsThinking { get; set; }
        public double TotalTime { get; set; }
        public double RemainTime { get; set; }
        public double MoveTime { get; set; }
        public double RemainMoveTime { get; set; }
    }
    public class RequestGamePlay : BaseRequest
    {
        public Guid PlayerId { get; set; }
        public Guid BoardId { get; set; }
    }
   
    public class RequestPickItem : RequestGamePlay
    {
        public string Color { get; set; }
        public string Type { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
   
    public class RequestMoveItem : RequestGamePlay
    {
        public string FromColor { get; set; }
        public string FromType { get; set; }
        public int FromRow { get; set; }
        public int FromCol { get; set; }

        public string ToColor { get; set; }
        public string ToType { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
    }

    public class RequestPickChessItem : RequestGamePlay
    {
        public string Color { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
    public class RequestMoveChessItem : RequestGamePlay
    {
        public string Color { get; set; }
        public Position From { get; set; }
        public Position To { get; set; }
    }
    public class DOChessItemMovement
    {
        public string Color { get; set; }
        public string Type { get; set; }
        public Position From { get; set; }
        public Position To { get; set; }
        public DOItemSignature Target { get; set; }
        public int CheckMateCount { get; set; }
        public bool IsGameOver { get; set; }
        public string WarningSide { get; set; }
        public string WarningMessage { get; set; }
    }
    public class DOItemSignature
    {
        public string Color { get; set; }
        public string Type { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
    public class DOPlayerInBoard
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Rank { get; set; }
        public int Score { get; set; }
        public bool ReadyToPlay { get; set; }
        public double TotalTime { get; set; }
        public double RemainTime { get; set; }
        public double TotalMoveTime { get; set; }
        public double RemainMoveTime { get; set; }
        public bool IsYourTurn { get; set; }
    }
    public class DOBoardShortcut
    {
        public Guid Id { get; set; }
        public string Room { get; set; }
        public string RedPlayerName { get; set; }
        public string BlackPlayerName { get; set; }
    }
    public class DOBoard
    {
        public Guid Id { get; set; }
        public string Room { get; set; }
        public virtual DOPlayerInBoard Owner { get; set; }
        public virtual DOPlayerInBoard RedPlayer { get; set; }
        public virtual DOPlayerInBoard BlackPlayer { get; set; }
        public string Status { get; set; }
        public double TotalTime { get; set; }
        public double AddingTime { get; set; }
        public string Result { get; set; }
        public string Turn { get; set; }
        public List<DOItem> Items { get; set; }
    }
    public class DOBoardSnapshot
    {
        public Guid Id { get; set; }
        public List<DOItem> Items { get; set; }
        public string Turn { get; set; }
    }
    public class DOPosition
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class DOItem
    {
        public string Color { get; set; }
        public string Type { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public bool Alive { get; set; }
        public List<DOPosition> Scope { get; set; }
    }
    public class DOMove
    {
        public DOItem Item { get; set; } 
        public Position Destination { get; set; }
        public DOItem Kill { get; set; }
        public DOPlayerInBoard RedPlayer { get; set; }
        public DOPlayerInBoard BlackPlayer { get; set; }
        public string Turn { get; set; }
        public bool IsCheckMate { get; set; }
        public bool IsGameOver { get; set; }
        public string BoardStatus { get; set; }
        public string WarningSide { get; set; }
        public string WarningMessage { get; set; }
    }
    public class DOGameOver
    {
        public string Result { get; set; }
        public int Score { get; set; }
        public string Description { get; set; }
    }
    public class RequestAcceptDraw: RequestGamePlay
    {
        public bool Accept { get; set; }
    }
}
