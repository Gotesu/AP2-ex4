using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageService.Infrastructure
{
    public class ImageServiceConfig
    {
        public List<string> handlers { get; set; }
        public int thumbSize { get; set; }
        public string source { get; set; }
        public string logName { get; set; }
        public string OPD { get; set; }

		public ImageServiceConfig(List<string> handlers, int thumbSize,
			string source, string logName, string OPD)
		{
			this.handlers = handlers;
			this.thumbSize = thumbSize;
			this.source = source;
			this.logName = logName;
			this.OPD = OPD;
		}

		public void Copy(ImageServiceConfig config)
		{
			this.handlers = config.handlers;
			this.thumbSize = config.thumbSize;
			this.source = config.source;
			this.logName = config.logName;
			this.OPD = config.OPD;
		}

		public string ToJSON()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public static ImageServiceConfig FromJSON(string str)
        {
            return JsonConvert.DeserializeObject<ImageServiceConfig>(str);
        }
    }
}
