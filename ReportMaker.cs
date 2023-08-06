using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Delegates.Reports
{
    public interface IStatisticMaker<T>
	{
		string Caption { get; }
		T MakeStatistic(IEnumerable<double> data);
	}

    public class MeanAndStdStatisticMaker : IStatisticMaker<MeanAndStd>
    {
        public string Caption => "Mean and Std";

        public MeanAndStd MakeStatistic(IEnumerable<double> data)
        {
            var list = data.ToList();
            var mean = data.Average();
            var std = Math.Sqrt(data.Select(z => Math.Pow(z - mean, 2)).Sum() / (list.Count - 1));

            return new MeanAndStd
            {
                Mean = mean,
                Std = std
            };
        }
    }

    public class MedianStatisticMaker : IStatisticMaker<double>
    {
        public string Caption => "Median";

        public double MakeStatistic(IEnumerable<double> data)
        {
            var list = data.OrderBy(z => z).ToList();
            if (list.Count % 2 == 0)
                return (list[list.Count / 2] + list[list.Count / 2 - 1]) / 2;

            return list[list.Count / 2];
        }
    }

	public interface ITemplateMaker
	{
        string MakeCaption(string caption);
        string BeginList();
        string MakeItem(string valueType, string entry);
        string EndList();
    }

    public class HtmlTemplateMaker : ITemplateMaker
    {
        
        public string MakeCaption(string caption)
        {
            return $"<h1>{caption}</h1>";
        }

        public string BeginList()
        {
            return "<ul>";
        }

        public string EndList()
        {
            return "</ul>";
        }

        public string MakeItem(string valueType, string entry)
        {
            return $"<li><b>{valueType}</b>: {entry}";
        }
    }

	public class MarkdownTemplateMaker : ITemplateMaker
	{
        public string BeginList()
        {
            return "";
        }

        public string EndList()
        {
            return "";
        }

        public string MakeCaption(string caption)
        {
            return $"## {caption}\n\n";
        }

        public string MakeItem(string valueType, string entry)
        {
            return $" * **{valueType}**: {entry}\n\n";
        }
    }

    public class ReportMaker<T>
    {
        protected ITemplateMaker templateMaker;
        protected IStatisticMaker<T> statisticMaker;

        public ReportMaker(ITemplateMaker templateMaker, IStatisticMaker<T> statisticMaker)
        {
            this.templateMaker = templateMaker;
            this.statisticMaker = statisticMaker;
        }

        public virtual string MakeReport(IEnumerable<Measurement> measurements)
        {
            var data = measurements.ToList();
            var result = new StringBuilder();
            result.Append(templateMaker.MakeCaption(statisticMaker.Caption));
            result.Append(templateMaker.BeginList());
            result.Append(templateMaker.MakeItem("Temperature", statisticMaker.MakeStatistic(data.Select(z => z.Temperature)).ToString()));
            result.Append(templateMaker.MakeItem("Humidity", statisticMaker.MakeStatistic(data.Select(z => z.Humidity)).ToString()));
            result.Append(templateMaker.EndList());
            return result.ToString();
        }
    }

    public static class ReportMakerHelper
	{
		public static string MeanAndStdHtmlReport(IEnumerable<Measurement> measurements)
		{
            return new ReportMaker<MeanAndStd>(new HtmlTemplateMaker(), new MeanAndStdStatisticMaker()).MakeReport(measurements);
        }

		public static string MedianMarkdownReport(IEnumerable<Measurement> measurements)
		{
            return new ReportMaker<double>(new MarkdownTemplateMaker(), new MedianStatisticMaker()).MakeReport(measurements);
        }

		public static string MeanAndStdMarkdownReport(IEnumerable<Measurement> measurements)
		{
            return new ReportMaker<MeanAndStd>(new MarkdownTemplateMaker(), new MeanAndStdStatisticMaker()).MakeReport(measurements);
        }

		public static string MedianHtmlReport(IEnumerable<Measurement> measurements)
		{
            return new ReportMaker<double>(new HtmlTemplateMaker(), new MedianStatisticMaker()).MakeReport(measurements);
        }
	}
}
