using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Drawing;
using Giselle.Drawing.Drawing;

namespace Giselle.DoujinshiDownloader.Forms.Utils
{
    public static class DrawingUtils2
    {
        public static Rectangle PlaceByDirection(Rectangle master, Size slave, PlaceDirection direction)
        {
            return new Rectangle(DrawingUtils.PlaceByDirection(master, slave, direction), slave);
        }

        public static Rectangle PlaceByDirection(Rectangle master, Size slave, PlaceDirection direction, int margin)
        {
            return new Rectangle(DrawingUtils.PlaceByDirection(master, slave, direction, margin), slave);
        }

        public static Rectangle PlaceByDirection(Rectangle master, Size slave, PlaceDirection direction, PlaceLevel level)
        {
            return new Rectangle(DrawingUtils.PlaceByDirection(master, slave, direction, level), slave);
        }

        public static Rectangle PlaceByDirection(Rectangle master, Size slave, PlaceDirection direction, PlaceLevel level, int margin)
        {
            return new Rectangle(DrawingUtils.PlaceByDirection(master, slave, direction, level, margin), slave);
        }

    }

}
