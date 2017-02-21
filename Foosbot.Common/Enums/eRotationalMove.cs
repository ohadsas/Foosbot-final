﻿// **************************************************************************************
// **																				   **
// **		(C) FOOSBOT - Final Software Engineering Project, 2015 - 2016			   **
// **		(C) Authors: M.Toubian, M.Shimon, E.Kleinman, O.Sasson, J.Gleyzer          **
// **			Advisors: Mr.Resh Amit & Dr.Hoffner Yigal							   **
// **		The information and source code here belongs to Foosbot project			   **
// **		and may not be reproduced or used without authors explicit permission.	   **
// **																				   **
// **************************************************************************************

namespace Foosbot.Common.Enums
{
    /// <summary>
    /// Player rotational position/move.
    /// </summary>
    public enum eRotationalMove : int
    {
        /// <summary>
        /// Undefined player rotational position
        /// </summary>
        NA = 0,

        /// <summary>
        /// Player is in 0 degrees (Legs back)
        /// </summary>
        RISE = 3, 

        /// <summary>
        /// Player is in 90 degrees (Legs down)
        /// </summary>
        DEFENCE = 2,

        /// <summary>
        /// Player is in 180 degrees (Legs ahead to the competitors gate)
        /// Also called Reverse-Rise
        /// </summary>
        KICK = 1
    }
}
