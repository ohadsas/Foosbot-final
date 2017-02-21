﻿// **************************************************************************************
// **																				   **
// **		(C) FOOSBOT - Final Software Engineering Project, 2015 - 2016			   **
// **		(C) Authors: M.Toubian, M.Shimon, E.Kleinman, O.Sasson, J.Gleyzer          **
// **			Advisors: Mr.Resh Amit & Dr.Hoffner Yigal							   **
// **		The information and source code here belongs to Foosbot project			   **
// **		and may not be reproduced or used without authors explicit permission.	   **
// **																				   **
// **************************************************************************************

using EasyLog;
using Foosbot.Common.Data;
using Foosbot.Common.Enums;
using Foosbot.Common.Logs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Foosbot
{
    /// <summary>
    /// Marks Static Class to display in UI
    /// </summary>
    public static class Marks
    {
        public static Point PlayerPosition(eRod type)
        {
            eMarks mark = eMarks.NA;
            switch (type)
            {
                case eRod.GoalKeeper:
                    mark = eMarks.GoalKeeper;
                    break;
                case eRod.Defence:
                    mark = eMarks.DefencePlayer1;
                    break;
                case eRod.Midfield:
                    mark = eMarks.MidfieldPlayer1;
                    break;
                case eRod.Attack:
                    mark = eMarks.AttackPlayer1;
                    break;
                default:
                    break;
            }

            if (_playerPositions.ContainsKey(mark))
            {
                object token = _positionToken[mark];
                Point? point = null;
                lock(token)
                {
                    point = new Point(_playerPositions[mark].X, _playerPositions[mark].Y);
                }

                if (point != null)
                    return (Point)point;
            }
            throw new NotSupportedException("Not supported player type");
        }
        

        private static Dictionary<eMarks, Point> _playerPositions = new Dictionary<eMarks, Point>()
        {
            { eMarks.GoalKeeper, new Point(0,0) },
            { eMarks.DefencePlayer1, new Point(0,0) },
            { eMarks.MidfieldPlayer1, new Point(0,0) },
            { eMarks.AttackPlayer1, new Point(0,0) }
        };

        private static Dictionary<eMarks, object> _positionToken = new Dictionary<eMarks, object>()
        {
             { eMarks.GoalKeeper, new object() },
            { eMarks.DefencePlayer1, new object() },
            { eMarks.MidfieldPlayer1, new object() },
            { eMarks.AttackPlayer1, new object() }
        };




        /// <summary>
        /// Holds the width rate of the table draw on the canvas
        /// </summary>
        private static double _actualWidthRate;

        /// <summary>
        /// Holds the height rate of the table draw on the canvas
        /// </summary>
        private static double _actualHeightRate;

        /// <summary>
        /// The canvas object 
        /// </summary>
        private static Canvas _canvas;

        /// <summary>
        /// Dispatcher for drawing on the canvas in the UI thread
        /// </summary>
        private static Dispatcher _dispatcher;

        /// <summary>
        /// Holds the maximum of the y coord in the device world
        /// </summary>
        private static double DEVICE_MAX_Y;

        /// <summary>
        /// Holds the maximum of the x coord in the device world
        /// </summary>
        private static double DEVICE_MAX_X;

        /// <summary>
        /// Holds the real life table max y in MM
        /// </summary>
        private static double TABLE_MAX_Y_MM;

        /// <summary>
        /// Holds the real life table max x in MM
        /// </summary>
        private static double TABLE_MAX_X_MM;

        /// <summary>
        /// The main dictionary that holds all the marks
        /// </summary>
        private static Dictionary<eMarks, int> _rods;

        /// <summary>
        /// Holds the number of player for each rod
        /// </summary>
        private static Dictionary<eMarks, int> _rodPlayerCount;

        /// <summary>
        /// Holds the distance between 2 players in each rod
        /// </summary>
        private static Dictionary<eMarks, int> _rodsPlayersDistance;

        /// <summary>
        /// Holds the offset of the first player for each rod from the stopper
        /// </summary>
        private static Dictionary<eMarks, int> _rodsOffsetY;
        
        /// <summary>
        /// Markup shape dictionary
        /// Contains markup key and related shape
        /// </summary>
        private static Dictionary<int, FrameworkElement> _markups;

        /// <summary>
        /// Init the Marks 
        /// </summary>
        public static void Initialize(Dispatcher dispatcher, Canvas canvas, double actualWidthRate, double actualHeightRate)
        {
            DEVICE_MAX_Y = Configuration.Attributes.GetValue<double>(Configuration.Names.FOOSBOT_AXE_Y_SIZE);
            DEVICE_MAX_X = Configuration.Attributes.GetValue<double>(Configuration.Names.FOOSBOT_AXE_X_SIZE);

            TABLE_MAX_Y_MM = Configuration.Attributes.GetValue<double>(Configuration.Names.TABLE_HEIGHT);
            TABLE_MAX_X_MM = Configuration.Attributes.GetValue<double>(Configuration.Names.TABLE_WIDTH);

            _rods = new Dictionary<eMarks, int>();
            _rodPlayerCount = new Dictionary<eMarks, int>();
            _rodsPlayersDistance = new Dictionary<eMarks, int>();
            _rodsOffsetY = new Dictionary<eMarks, int>();

            foreach (eRod rodType in Enum.GetValues(typeof(eRod)))
	        {
                int rodPlayersCount = Configuration.Attributes.GetPlayersCountPerRod(rodType);
                int yDistance = Configuration.Attributes.GetPlayersDistancePerRod(rodType);
                int firstPlayerOffsetY = Configuration.Attributes.GetPlayersOffsetYPerRod(rodType);

                int x = Configuration.Attributes.GetRodXCoordinate(rodType);

                eMarks mark;
                Enum.TryParse<eMarks>(rodType.ToString(), out mark);
                if (!mark.Equals(eMarks.NA))
                {
                    _rods.Add(mark, x);
                    _rodPlayerCount.Add(mark, rodPlayersCount);
                    _rodsPlayersDistance.Add(mark, yDistance);
                    _rodsOffsetY.Add(mark, firstPlayerOffsetY);
                }
	        }

            _canvas = canvas;
            _actualWidthRate = actualWidthRate;
            _actualHeightRate = actualHeightRate;
            _dispatcher = dispatcher;

            _markups = new Dictionary<int, FrameworkElement>();
            foreach (eMarks mark in Enum.GetValues(typeof(eMarks)))
            {
                switch (mark)
                {
                    case eMarks.BallMark:
                    case eMarks.ButtomLeftMark:
                    case eMarks.ButtomRightMark:
                    case eMarks.TopLeftMark:
                    case eMarks.TopRightMark:
                    case eMarks.RicochetMark:
                    case eMarks.GoalKeeperPlayer:
                    case eMarks.DefencePlayer1:
                    case eMarks.DefencePlayer2:
                    case eMarks.MidfieldPlayer1:
                    case eMarks.MidfieldPlayer2:
                    case eMarks.MidfieldPlayer3:
                    case eMarks.MidfieldPlayer4:
                    case eMarks.MidfieldPlayer5:
                    case eMarks.AttackPlayer1:
                    case eMarks.AttackPlayer2:
                    case eMarks.AttackPlayer3:
                        _markups.Add((int)mark, new Ellipse());
                        break;
                    case eMarks.GoalKeeperPlayerRect:
                    case eMarks.DefencePlayer1Rect:
                    case eMarks.DefencePlayer2Rect:
                    case eMarks.MidfieldPlayer1Rect:
                    case eMarks.MidfieldPlayer2Rect:
                    case eMarks.MidfieldPlayer3Rect:
                    case eMarks.MidfieldPlayer4Rect:
                    case eMarks.MidfieldPlayer5Rect:
                    case eMarks.AttackPlayer1Rect:
                    case eMarks.AttackPlayer2Rect:
                    case eMarks.AttackPlayer3Rect:
                        _markups.Add((int)mark, new Rectangle());
                        break;
                    case eMarks.ButtomLeftText:
                    case eMarks.ButtomRightText:
                    case eMarks.TopLeftText:
                    case eMarks.TopRightText:
                        _markups.Add((int)mark, new TextBlock());
                        break;
                    case eMarks.BallVectorArrow:                  
                        _markups.Add((int)mark, new Line());
                        _markups.Add((int)eMarks.BallVectorArrow + 1, new Line());
                        _markups.Add((int)eMarks.BallVectorArrow + 2, new Line());
                        break;
                    case eMarks.LeftBorder:
                    case eMarks.RightBorder:
                    case eMarks.TopBorder:
                    case eMarks.BottomBorder:
                    case eMarks.GoalKeeper:
                    case eMarks.Defence:
                    case eMarks.Midfield:
                    case eMarks.Attack:
                    case eMarks.GoalKeeperSector1:
                    case eMarks.GoalKeeperSector2:
                    case eMarks.DefenceSector1:
                    case eMarks.DefenceSector2:
                    case eMarks.MidfieldSector1:
                    case eMarks.MidfieldSector2:
                    case eMarks.AttackSector1:
                    case eMarks.AttackSector2:
                        _markups.Add((int)mark, new Line());
                        break;
                }
            }

            foreach (FrameworkElement element in _markups.Values)
            {
                _canvas.Children.Add(element);
            }
        }

        /// <summary>
        /// Convert coord points to location points
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        private static void ConvertToLocation(ref int x, ref int y)
        {
            double outX, outY;
            TransformAgent.Data.InvertTransform(x, y, out outX, out outY);
            x = Convert.ToInt32(outX);
            y = Convert.ToInt32(outY);

        }

        /// <summary>
        /// Convert coord points to location points
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        private static void ConvertToLocation(int x, ref int y)
        {
            double outX, outY;
            TransformAgent.Data.InvertTransform(x, y, out outX, out outY);
            y = Convert.ToInt32(outY);
        }


        /// <summary>
        /// Convert coord points to location points
        /// </summary>
        /// <param name="x">X coord</param>
        /// <param name="y">Y coord</param>
        private static void ConvertToLocation(ref int x,int y)
        {
            double outX, outY;
            TransformAgent.Data.InvertTransform(x, y, out outX, out outY);
            x = Convert.ToInt32(outY);
        }
        /// <summary>
        /// Convert the a given x from MM to Device coord
        /// </summary>
        /// <param name="x">X in MM</param>
        /// <returns>X in Coords</returns>
        public static int XTableToDeviceCoordinates(int x)
        {
            return Convert.ToInt32((x * DEVICE_MAX_X) / TABLE_MAX_X_MM);
        }

        /// <summary>
        /// Convert the a given y from MM to Device coord
        /// </summary>
        /// <param name="x">Y in MM</param>
        /// <returns>Y in Coords</returns>
        public static int YTableToDeviceCoordinates(int y)
        {
            return Convert.ToInt32((y * DEVICE_MAX_Y) / TABLE_MAX_Y_MM);
        }

        /// <summary>
        /// Draw the ricochet point of the ball on the border of the table 
        /// </summary>
        /// <param name="x">X coord of the ricochet on the canvas</param>
        /// <param name="y">Y coord of the ricochet on the canvas</param>
        /// <param name="isLocation">Bool that convert between location and coord : optional</param>
        /// <param name="circleColor">The color of the stroke of the ricochet mark : optional</param>
        public static void DrawRicochetMark(int x, int y, bool isLocation = false, SolidColorBrush circleColor = null)
        {
            try
            {
                if (isLocation) ConvertToLocation(ref x, ref y);

                const int radius = 10;
                const int key = (int)eMarks.RicochetMark;
                Point presentationCenter = new Point(x * _actualWidthRate, y * _actualHeightRate);
                int presentationRadius = Convert.ToInt32(radius * ((_actualWidthRate + _actualHeightRate) / 2));

                _dispatcher.Invoke(new ThreadStart(delegate
                {
                    (_markups[key] as Shape).StrokeThickness = 2;
                    (_markups[key] as Shape).Stroke = (circleColor == null) ? Brushes.Chocolate : circleColor;
                    _markups[key].Width = presentationRadius * 2;
                    _markups[key].Height = presentationRadius * 2;
                    Canvas.SetLeft(_markups[key], presentationCenter.X - presentationRadius);
                    Canvas.SetTop(_markups[key], presentationCenter.Y - presentationRadius);
                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw ricochet mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Show table borders calculated by IP Unit
        /// </summary>
        /// <param name="corners"></param>
        public static void DrawTableBorders(Dictionary<eCallibrationMark, Emgu.CV.Structure.CircleF> marks)
        {
            try
            {
                Point[] corners = new Point[]
                    {
                        new Point(marks[eCallibrationMark.BL].Center.X, marks[eCallibrationMark.BL].Center.Y),
                        new Point(marks[eCallibrationMark.TL].Center.X, marks[eCallibrationMark.TL].Center.Y),
                        new Point(marks[eCallibrationMark.TR].Center.X, marks[eCallibrationMark.TR].Center.Y),
                        new Point(marks[eCallibrationMark.BR].Center.X, marks[eCallibrationMark.BR].Center.Y)
                    };

                const int key = (int)eMarks.LeftBorder;
                for (int start = 0; start < 4; start++)
                {
                    int end = (start + 1 > 3) ? 0 : start + 1;

                    _dispatcher.Invoke(new ThreadStart(delegate
                    {
                        (_markups[key + start] as Shape).StrokeThickness = 2;
                        (_markups[key + start] as Shape).Stroke = Brushes.Pink;

                        (_markups[key + start] as Line).X1 = corners[start].X * _actualWidthRate;
                        (_markups[key + start] as Line).Y1 = corners[start].Y * _actualHeightRate;
                        (_markups[key + start] as Line).X2 = corners[end].X * _actualWidthRate;
                        (_markups[key + start] as Line).Y2 = corners[end].Y * _actualHeightRate;

                        Canvas.SetLeft(_markups[key + start], 0);
                        Canvas.SetTop(_markups[key + start], 0);
                    }));
                }
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw table borders. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Init the rods lines on the canvas
        /// </summary>
        /// <param name="thickness">Optional thickness of the rods [default : 6]</param>
        /// <param name="isLocation">Optional isLocation [default : true]</param>
        public static void DrawRods(int thickness = 6, bool isLocation = true)
        {
            try
            {
                _dispatcher.Invoke(new ThreadStart(()=>
                {
                    const int key = (int)eMarks.GoalKeeper;

                    for (int eMarksCounter = 0; eMarksCounter < 4; eMarksCounter++)
                    {
                        
                        int buttomX = XTableToDeviceCoordinates(_rods[(eMarksCounter + eMarks.GoalKeeper)]);
                        int buttomY = ((int)DEVICE_MAX_Y);
                        int topX = buttomX;
                        int topY = 0;

                        if (isLocation)
                        {
                            ConvertToLocation(ref buttomX, ref buttomY);
                            ConvertToLocation(ref topX, ref topY);
                        }

                        (_markups[key + eMarksCounter] as Shape).StrokeThickness = thickness;
                        (_markups[key + eMarksCounter] as Shape).Stroke = Brushes.White;
                        (_markups[key + eMarksCounter] as Line).X1 = topX * _actualWidthRate;
                        (_markups[key + eMarksCounter] as Line).Y1 = topY * _actualHeightRate;
                        (_markups[key + eMarksCounter] as Line).X2 = buttomX * _actualWidthRate;
                        (_markups[key + eMarksCounter] as Line).Y2 = buttomY * _actualHeightRate;

                        Canvas.SetLeft(_markups[key + eMarksCounter], 0);
                        Canvas.SetTop(_markups[key + eMarksCounter], 0);
                    }
                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw rods marks. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }


        /// <summary>
        /// Draw the given rod dynamic sector
        /// </summary>
        /// <param name="rod">The rod for sector calculation</param>
        /// <param name="dynamicSectorWidth">The dynamic sector of the rod</param>
        /// <param name="thickness">Optional thickness of the sector line [default : 2]</param>
        /// <param name="isLocation">Optional isLocation [default : true]</param>
        public static void DrawSector(eRod rod,int dynamicSectorWidth,int thickness = 2, bool isLocation = true)
        {
            try
            {
                Brush color = null;
                eMarks mark;              
                Enum.TryParse<eMarks>(rod.ToString(), out mark);

                int key = (int)mark * 10;
                int x = XTableToDeviceCoordinates(_rods[mark]);

                int sectorStart = Convert.ToInt32(x - dynamicSectorWidth / 2.0);
                int sectorStartTop = sectorStart;
                int sectorStartButtom = sectorStart;

                int sectorEnd = Convert.ToInt32(x + dynamicSectorWidth / 2.0);
                int sectorEndTop = sectorEnd;
                int sectorEndButtom = sectorEnd; 

                int yTop = 0;
                int yButtom = ((int)DEVICE_MAX_Y);

                _dispatcher.Invoke(new ThreadStart(delegate
                {

                    switch (mark)
                    {
                        case eMarks.GoalKeeper: color = Brushes.Yellow; break;
                        case eMarks.Defence: color = Brushes.Pink; break;
                        case eMarks.Midfield: color = Brushes.Red; break;
                        case eMarks.Attack: color = Brushes.DarkRed; break;
                        default: break;
                    }

                    if (isLocation)
                    {
                        int yButtomTemp = yButtom;
                        int yTopTemp = yTop;
                        ConvertToLocation(ref sectorStartTop, ref yTop);
                        ConvertToLocation(ref sectorStartButtom, ref yButtom);
                        ConvertToLocation(ref sectorEndTop, ref yTopTemp);
                        ConvertToLocation(ref sectorEndButtom, ref yButtomTemp);
                    }

                    (_markups[key] as Shape).StrokeThickness = thickness;
                    (_markups[key] as Shape).Stroke = color;
                    (_markups[key] as Line).X1 = sectorStartTop * _actualWidthRate;
                    (_markups[key] as Line).Y1 = yTop * _actualHeightRate;
                    (_markups[key] as Line).X2 = sectorStartButtom * _actualWidthRate;
                    (_markups[key] as Line).Y2 = yButtom * _actualHeightRate;

                    (_markups[key + 1] as Shape).StrokeThickness = thickness;
                    (_markups[key + 1] as Shape).Stroke = color;
                    (_markups[key + 1] as Line).X1 = sectorEndTop * _actualWidthRate;
                    (_markups[key + 1] as Line).Y1 = yTop * _actualHeightRate;
                    (_markups[key + 1] as Line).X2 = sectorEndButtom * _actualWidthRate;
                    (_markups[key + 1] as Line).Y2 = yButtom * _actualHeightRate;

                    Canvas.SetLeft(_markups[key], 0);
                    Canvas.SetTop(_markups[key], 0); 
                    Canvas.SetLeft(_markups[key + 1], 0);
                    Canvas.SetTop(_markups[key + 1], 0);

                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw rod dynamic sector marks. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Draw a rod by a given eMark sign , moving the rod on the linear and rotational axes
        /// </summary>
        /// <param name="rod">eMark of the wanted rod : GoalKeeper, Defence , Midfield, Attack</param>
        /// <param name="deltaYMovment">The change on the linear movement</param>
        /// <param name="rotationalMove">eRotationalMove of the rod : DEFENCE, RISE, ATTACK</param>
        public static void DrawRodPlayers(eRod rod, int linearMoveDestination, eRotationalMove rotationalMove ,bool isLocation = true)
        {
            try
            {
                eMarks playersBase = 0;
                eMarks mark;
                Enum.TryParse<eMarks>(rod.ToString(), out mark);

                int eMarkType = ((int)mark);
                int rodPlayersCount = _rodPlayerCount[mark];
                int yDistance = _rodsPlayersDistance[mark];
                int firstPlayerOffsetY = _rodsOffsetY[mark];
                int x = XTableToDeviceCoordinates(_rods[mark]);
                



                switch (eMarkType)
                {
                    case (int)eMarks.GoalKeeper: playersBase = eMarks.GoalKeeperPlayer; break;
                    case (int)eMarks.Defence: playersBase = eMarks.DefencePlayer1; break;
                    case (int)eMarks.Midfield: playersBase = eMarks.MidfieldPlayer1; break;
                    case (int)eMarks.Attack: playersBase = eMarks.AttackPlayer1; break;
                    default: break;
                }

                for (int rodPlayer = 0; rodPlayer < rodPlayersCount; rodPlayer++)
                {
                    int y = linearMoveDestination + firstPlayerOffsetY + yDistance * rodPlayer;
                    int y1 = y;
                    int x1 = x;
                    if (isLocation)
                    {
                        ConvertToLocation(ref x1, ref y);
                    }
                    DrawPlayer(playersBase + rodPlayer, new Point(x1 * _actualWidthRate, y1 * _actualHeightRate), 12, rotationalMove);
                }
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw rods players mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        //public static void DrawRodPlayers(eRod rod, int linearMoveDestination, eRotationalMove rotationalMove, bool isLocation = true)
        //{
        //    try
        //    {
        //        eMarks playersBase = 0;
        //        eMarks mark;
        //        Enum.TryParse<eMarks>(rod.ToString(), out mark);
        //        int eMarkType = ((int)mark);
        //        int rodPlayersCount = _rodPlayerCount[mark];
        //        int yDistance = _rodsPlayersDistance[mark];
        //        int firstPlayerOffsetY = _rodsOffsetY[mark];
        //        int rodStartX = XTableToDeviceCoordinates(_rods[mark]);
        //        switch (eMarkType)
        //        {
        //            case (int)eMarks.GoalKeeper: playersBase = eMarks.GoalKeeperPlayer; break;
        //            case (int)eMarks.Defence: playersBase = eMarks.DefencePlayer1; break;
        //            case (int)eMarks.Midfield: playersBase = eMarks.MidfieldPlayer1; break;
        //            case (int)eMarks.Attack: playersBase = eMarks.AttackPlayer1; break;
        //            default: break;
        //        }

        //        for (int rodPlayer = 0; rodPlayer < rodPlayersCount; rodPlayer++)
        //        {
        //            int movmentOffset = 0;
        //            int x = rodStartX;
        //            int y = 0;
        //            int y1 = 0;
        //            if (isLocation)
        //            {
        //                ConvertToLocation(ref x, ref y1);
        //            }

        //            y = y1 + linearMoveDestination + firstPlayerOffsetY + yDistance * rodPlayer;
        //            DrawPlayer(playersBase + rodPlayer,
        //                new Point(x * _actualWidthRate, y * _actualHeightRate), 12, rotationalMove);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Common.Error(String.Format("[{0}] Failed to draw rods players mark. Reason: {1}",
        //                                                        MethodBase.GetCurrentMethod().Name, e.Message));
        //    }
        //}

        /// <summary>
        /// Drawing a single player at a time , used by the DrawRodPlayers method to draw all rods players
        /// </summary>
        /// <param name="eNumKey">eMark of the wanted rod : GOALKEEPER , Defence , Midfield, Attack</param>
        /// <param name="center">Center of the players radius</param>
        /// <param name="radius">Radius size</param>
        /// <param name="circleColor">Color for the roational current pos</param>
        private static void DrawPlayer(eMarks eNumKey,Point center, int radius,
                    eRotationalMove rotationalMove, SolidColorBrush playerColor = null)
        {
            try
            {
                object token = null;
                if (_positionToken.ContainsKey(eNumKey))
                    token = _positionToken[eNumKey];

                if (token != null)
                {
                    lock(token)
                    {
                        _playerPositions[eNumKey] = center;
                    }
                }

                int key = (int)eNumKey;
                int rotationalMoveFactor = 0;
                SolidColorBrush rotationalColor = Brushes.DarkBlue;

                if (rotationalMove == eRotationalMove.DEFENCE)
                {
                    rotationalMoveFactor = radius;
                    rotationalColor = Brushes.Blue;
                }
                else if (rotationalMove == eRotationalMove.RISE) 
                {
                    rotationalMoveFactor = (int)(radius * 2.5);
                    rotationalColor = Brushes.Cyan;
                }

                _dispatcher.Invoke(new ThreadStart(delegate
                {
                    (_markups[key] as Shape).Fill = (rotationalColor == null) ? Brushes.White : rotationalColor;

                    _markups[key].Width = radius * 2;
                    _markups[key].Height = radius * 2;

                    Canvas.SetLeft(_markups[key], center.X - radius);
                    Canvas.SetTop(_markups[key], center.Y - radius);

                    (_markups[key + 5] as Shape).Height = 24;
                    (_markups[key + 5] as Shape).Width = 30;
                    (_markups[key + 5] as Shape).StrokeThickness = 2;
                    (_markups[key + 5] as Shape).Fill = rotationalColor;

                    Canvas.SetLeft(_markups[key + 5], center.X - rotationalMoveFactor);
                    Canvas.SetTop(_markups[key + 5], center.Y - radius);

                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw player mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }


        /// <summary>
        /// Drawing the ball on the canvas
        /// </summary>
        /// <param name="center">Ball circle center</param>
        /// <param name="radius">Ball radius</param>
        /// <param name="circleColor">Ball color : optional</param>
        public static void DrawBall(Point center, int radius, SolidColorBrush circleColor = null)
        {
            try
            {
                const int key = (int)eMarks.BallMark;
                Point presentationCenter = new Point(center.X * _actualWidthRate, center.Y * _actualHeightRate);
                int presentationRadius = Convert.ToInt32(radius * ((_actualWidthRate + _actualHeightRate) / 2));

                _dispatcher.Invoke(new ThreadStart(delegate
                {
                    (_markups[key] as Shape).Fill = Brushes.White;
                    (_markups[key] as Shape).StrokeThickness = 2;
                    (_markups[key] as Shape).Stroke = (circleColor == null) ? Brushes.Red : circleColor;

                    _markups[key].Width = presentationRadius * 2;
                    _markups[key].Height = presentationRadius * 2;

                    Canvas.SetLeft(_markups[key], presentationCenter.X - presentationRadius);
                    Canvas.SetTop(_markups[key], presentationCenter.Y - presentationRadius);
                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw ball mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Draw the calibration circles on the canvas
        /// </summary>
        /// <param name="mark">The wanted calibration mark for drawing</param>
        /// <param name="center">Center of the given calibration mark</param>
        /// <param name="radius">Radius of the given calibration mark</param>
        /// <param name="circleColor">Optional color [default : Pink]</param>
        /// <param name="textColor">Optional text color [default : OrangeRed]</param>
        /// <param name="fontSize">Optional font size [default : 12 ]</param>
        /// <param name="text"></param>
        public static void DrawCallibrationCircle(eCallibrationMark mark, Point center, int radius,
            SolidColorBrush circleColor = null, SolidColorBrush textColor = null, double fontSize = 12,
                string text = "")
        {
            try
            {
                Point presentationCenter = new Point(center.X * _actualWidthRate, center.Y * _actualHeightRate);
                int presentationRadius = Convert.ToInt32(radius * ((_actualWidthRate + _actualHeightRate) / 2));

                _dispatcher.Invoke(new ThreadStart(delegate
                {
                    int markNum = 0;
                    int textNum = 0;
                    switch (mark)
                    {
                        case eCallibrationMark.BL:
                            markNum = (int)eMarks.ButtomLeftMark;
                            textNum = (int)eMarks.ButtomLeftText;
                            break;
                        case eCallibrationMark.BR:
                            markNum = (int)eMarks.ButtomRightMark;
                            textNum = (int)eMarks.ButtomRightText;
                            break;
                        case eCallibrationMark.TL:
                            markNum = (int)eMarks.TopLeftMark;
                            textNum = (int)eMarks.TopLeftText;
                            break;
                        case eCallibrationMark.TR:
                            markNum = (int)eMarks.TopRightMark;
                            textNum = (int)eMarks.TopRightText;
                            break;
                    }

                    (_markups[markNum] as Shape).StrokeThickness = 2;
                    (_markups[markNum] as Shape).Stroke = (circleColor == null) ? Brushes.Pink : circleColor;
                    (_markups[textNum] as TextBlock).FontSize = fontSize;
                    (_markups[textNum] as TextBlock).Text = (String.IsNullOrEmpty(text)) ? String.Format("{0}:{1}x{2}", mark, Convert.ToInt32(center.X), Convert.ToInt32(center.Y)) : text;
                    (_markups[textNum] as TextBlock).Foreground = (textColor == null) ? Brushes.OrangeRed : textColor;

                    _markups[markNum].Width = presentationRadius * 2;
                    _markups[markNum].Height = presentationRadius * 2;
                    Canvas.SetLeft(_markups[markNum], presentationCenter.X - presentationRadius);
                    Canvas.SetTop(_markups[markNum], presentationCenter.Y - presentationRadius);
                    Canvas.SetLeft(_markups[textNum], presentationCenter.X - presentationRadius);
                    Canvas.SetTop(_markups[textNum], presentationCenter.Y - presentationRadius);
                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw calibration mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }

        /// <summary>
        /// Drawing the vector of the ball
        /// </summary>
        /// <param name="center">Center point to draw vector from</param>
        /// <param name="vector">The end of the vector</param>
        /// <param name="isLocation">Optional is location [default : true]</param>
        /// <param name="color">Optional color [default : Aqua]</param>
        public static void DrawBallVector(Point center, Point vector, bool isLocation = true, SolidColorBrush color = null)
        {
            try
            {
                if (isLocation)
                {
                    int x = Convert.ToInt32(center.X);
                    int y = Convert.ToInt32(center.Y);
                    ConvertToLocation(ref x, ref y);
                    center = new Point(x, y);

                    x = Convert.ToInt32(vector.X);
                    y = Convert.ToInt32(vector.Y);

                    //we don't want to convert 0 of vector
                    int convX = x;  
                    int convY = y;
                    ConvertToLocation(ref convX, ref convY);
                    if (x!=0) x = convX;
                    if (y!=0) y = convY;
                    vector = new Point(x, y);
                }

                Point presentationStartPoint = new Point(center.X * _actualWidthRate, center.Y * _actualHeightRate);
                Point presentationVector = new Point(vector.X * _actualWidthRate, vector.Y * _actualHeightRate);
                Point presentationEndPoint = new Point(presentationStartPoint.X + presentationVector.X,
                    presentationStartPoint.Y + presentationVector.Y);
                const int key = (int)eMarks.BallVectorArrow;

                _dispatcher.Invoke(new ThreadStart(delegate
                {
                    for (int i = key; i <= key + 2; i++)
                    {
                        (_markups[i] as Line).StrokeThickness = 2;
                        (_markups[i] as Line).Stroke = (color == null) ? System.Windows.Media.Brushes.Aqua : color;
                        (_markups[i] as Line).X1 = presentationStartPoint.X;
                        (_markups[i] as Line).Y1 = presentationStartPoint.Y;
                    }
                    (_markups[key] as Line).X2 = presentationEndPoint.X;
                    (_markups[key] as Line).Y2 = presentationEndPoint.Y;

                    (_markups[key + 1] as Line).X2 = presentationEndPoint.X;
                    (_markups[key + 1] as Line).Y2 = presentationEndPoint.Y;
                    (_markups[key + 2] as Line).X2 = presentationEndPoint.X;
                    (_markups[key + 2] as Line).Y2 = presentationEndPoint.Y;

                    for (int i = key; i <= key /*+ 2*/; i++)
                    {
                        Canvas.SetLeft(_markups[i], 0);
                        Canvas.SetTop(_markups[i], 0);
                    }
                }));
            }
            catch (Exception e)
            {
                Log.Print(String.Format("Failed to draw ball vector mark. Reason: {0}", e.Message), eCategory.Error, LogTag.COMMON);
            }
        }
    }
}
