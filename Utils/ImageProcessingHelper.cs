﻿using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Visiomex.Projects.LabelingTool.Utils
{
    class ImageProcessingHelper
    {
        public static void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
        HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {
            HTuple hv_GenParamName = null, hv_GenParamValue = null;
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_CoordSystem_COPY_INP_TMP = hv_CoordSystem.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Column: The column coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically...
            //   - if |Row| == |Column| == 1: for each new textline
            //   = else for each text position.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow
            //       otherwise -> use given string as color string for the shadow color
            //
            //It is possible to display multiple text strings in a single call.
            //In this case, some restrictions apply:
            //- Multiple text positions can be defined by specifying a tuple
            //  with multiple Row and/or Column coordinates, i.e.:
            //  - |Row| == n, |Column| == n
            //  - |Row| == n, |Column| == 1
            //  - |Row| == 1, |Column| == n
            //- If |Row| == |Column| == 1,
            //  each element of String is display in a new textline.
            //- If multiple positions or specified, the number of Strings
            //  must match the number of positions, i.e.:
            //  - Either |String| == n (each string is displayed at the
            //                          corresponding position),
            //  - or     |String| == 1 (The string is displayed n times).
            //
            //
            //Convert the parameters for disp_text.
            if ((int)new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(new HTuple())).TupleOr(
                new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(new HTuple()))) != 0)
                return;
            
            if ((int)new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1)) != 0)
                hv_Row_COPY_INP_TMP = 12;

            if ((int)new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1)) != 0)
                hv_Column_COPY_INP_TMP = 12;

            //
            //Convert the parameter Box to generic parameters.
            hv_GenParamName = new HTuple();
            hv_GenParamValue = new HTuple();
            if ((int)new HTuple(new HTuple(hv_Box.TupleLength())).TupleGreater(0) != 0)
            {
                if ((int)new HTuple(hv_Box.TupleSelect(0).TupleEqual("false")) != 0)
                {
                    //Display no box
                    hv_GenParamName = hv_GenParamName.TupleConcat("box");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)new HTuple(hv_Box.TupleSelect(0).TupleNotEqual("true")) != 0)
                {
                    //Set a color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("box_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(0));
                }
            }
            if ((int)new HTuple(new HTuple(hv_Box.TupleLength()).TupleGreater(1)) != 0)
            {
                if ((int)new HTuple(hv_Box.TupleSelect(1).TupleEqual("false")) != 0)
                {
                    //Display no shadow.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)new HTuple(hv_Box.TupleSelect(1).TupleNotEqual("true")) != 0)
                {
                    //Set a shadow color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(1));
                }
            }
            //Restore default CoordSystem behavior.
            if ((int)new HTuple(hv_CoordSystem_COPY_INP_TMP.TupleNotEqual("window")) != 0)
            {
                hv_CoordSystem_COPY_INP_TMP = "image";
            }
            //
            if ((int)new HTuple(hv_Color_COPY_INP_TMP.TupleEqual("")) != 0)
            {
                //disp_text does not accept an empty string for Color.
                hv_Color_COPY_INP_TMP = new HTuple();
            }
            //
            HOperatorSet.DispText(hv_WindowHandle, hv_String, hv_CoordSystem_COPY_INP_TMP,
                hv_Row_COPY_INP_TMP, hv_Column_COPY_INP_TMP, hv_Color_COPY_INP_TMP, hv_GenParamName,
                hv_GenParamValue);

            return;
        }

    }
}
