using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.common
{

    public class LongRationalJsonConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(LongRational);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.Value is double)
                return new LongRational((double)reader.Value);
            return new LongRational();
        }

       

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            writer.WriteValue(value == null ? 0 : ((LongRational)value).FloatVal);
        }
    }
    public struct  LongRational
    {
        public const long DEFAULT_PRECISION = 10000;
        public const double DEFAULT_UNIT_VALUE = (double)1 / DEFAULT_PRECISION;
        long _num=0;
        long _denum=1;

        public LongRational()
        {
        }
        public LongRational(long n) : this(n, 1)
        {
        }
        public LongRational(long n,long d)
        {
            this.Num = n;
            this.Denum = d;
        }
        public long Num
        {
            get
            {
                return _num;
            }
            set
            {
                _num = value;
            }
        }
        public long Denum
        {
            get
            {
                return _denum;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("Denumerator should be positive");
                _denum = value;
            }
        }

        public double FloatVal => (double)this.Num / (double)this.Denum;

        public bool IsOne => this.Denum != 0 && this.Num == this.Denum;

        public LongRational(double val, long denum)
        {
            this.Denum = denum;
            this.Num = (long)Math.Round(denum * val);
        }
        public LongRational(double val):this(val, DEFAULT_PRECISION)
        {
        }
        public LongRational Add(LongRational x)
        {
            return new LongRational
            {
                Num = this.Num * x.Denum + this.Denum * x.Num,
                Denum=this.Denum * x.Denum
            };
        }
    }
}
