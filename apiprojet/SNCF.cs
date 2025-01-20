namespace ville;

    public class CGeo
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Geometry
    {
        public List<double> coordinates { get; set; }
        public string type { get; set; }
    }

    public class GeoPoint2d
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class GeoShape
    {
        public string type { get; set; }
        public Geometry geometry { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
    }

    public class Result
    {
        public string code_uic { get; set; }
        public string libelle { get; set; }
        public string fret { get; set; }
        public string voyageurs { get; set; }
        public string code_ligne { get; set; }
        public int rg_troncon { get; set; }
        public string pk { get; set; }
        public string commune { get; set; }
        public string departemen { get; set; }
        public int idreseau { get; set; }
        public string idgaia { get; set; }
        public double x_l93 { get; set; }
        public double y_l93 { get; set; }
        public double x_wgs84 { get; set; }
        public double y_wgs84 { get; set; }
        public CGeo c_geo { get; set; }
        public GeoPoint2d geo_point_2d { get; set; }
        public GeoShape geo_shape { get; set; }
    }

    public class Root
    {
        public int total_count { get; set; }
        public List<Result> results { get; set; }
    }






