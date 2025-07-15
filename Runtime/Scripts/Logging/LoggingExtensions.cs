using System.Globalization;
using UnityEngine;

namespace DistractorTask.Logging
{
    public static class LoggingExtensions
    {
        
        
        public const bool OptimizeForExcel = true;
        
        public const char Delimiter = LoggingExtensions.OptimizeForExcel ? ';' : ',';
        
        public static Vector3 ReadVector3FromCSV(this string vectorRepresentation, char delimiter = ',')
        {
            var parts = vectorRepresentation.Split(delimiter);
            return new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
        }

        public static Vector2 ReadVector2FromCSV(this string vectorRepresentation, char delimiter = ',')
        {
            var parts = vectorRepresentation.Split(delimiter);
            return new Vector2(float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture));
        }

        public static Quaternion ReadQuaternionFromCSV(this string quaternionRepresentation, char delimiter = ',')
        {
            var parts = quaternionRepresentation.Split(delimiter);
            return new Quaternion(float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture), float.Parse(parts[3], CultureInfo.InvariantCulture));
        }
        
        public static string WriteVector3ToCSVString(this Vector3 vector, char delimiter = ',')
        {
            return
                $"{vector.x.ToString(CultureInfo.InvariantCulture)}{delimiter}{vector.y.ToString(CultureInfo.InvariantCulture)}{delimiter}{vector.z.ToString(CultureInfo.InvariantCulture)}";
        }
        
        public static string WriteVector2ToCSVString(this Vector2 vector, char delimiter = ',')
        {
            return
                $"{vector.x.ToString(CultureInfo.InvariantCulture)}{delimiter}{vector.y.ToString(CultureInfo.InvariantCulture)}";
        }

        public static string WriteQuaternionToCSVString(this Quaternion quaternion, char delimiter = ',')
        {
            return
                $"{quaternion.x.ToString(CultureInfo.InvariantCulture)}{delimiter}{quaternion.y.ToString(CultureInfo.InvariantCulture)}{delimiter}{quaternion.z.ToString(CultureInfo.InvariantCulture)}{delimiter}{quaternion.w.ToString(CultureInfo.InvariantCulture)}";
        }
        
        public static string SanitizeCSVLine(this string message)
        {
            if (OptimizeForExcel)
            {
                return message.Replace(',', ';');
            }
            return message.Replace(';', ',');
        }

        public static string SanitizeCSVElement(this string element)
        {
            if (OptimizeForExcel)
            {
                return element.Replace(';', ',');
            }
            return element.Replace(',', ';');
        }
    }
}