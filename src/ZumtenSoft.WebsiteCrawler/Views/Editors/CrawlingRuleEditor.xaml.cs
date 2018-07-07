using System;
using System.Windows;
using System.Windows.Controls;
using ZumtenSoft.WebsiteCrawler.BusLayer.Models.Configuration;

namespace ZumtenSoft.WebsiteCrawler.Views.Editors
{
    public partial class CrawlingRuleEditor : Window
    {
        public CrawlingConfig Config
        {
            get { return (CrawlingConfig)GetValue(ConfigProperty); }
            set { SetValue(ConfigProperty, value); }
        }

        public static readonly DependencyProperty ConfigProperty = DependencyProperty.Register(
            "Config", typeof(CrawlingConfig), typeof(CrawlingRuleEditor));


        public bool ApplyChanges { get; private set; }

        public CrawlingRuleEditor(CrawlingConfig config, CrawlingRule rule)
        {
            DataContext = rule;
            Config = config;
            InitializeComponent();
        }

        public static bool StartEditing(Window parent, CrawlingConfig config, CrawlingRule rule)
        {
            CrawlingRuleEditor view = new CrawlingRuleEditor(config, rule);
            view.Owner = parent;
            view.ShowDialog();

            return view.ApplyChanges;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            //ErrorProvider.
            //ReadOnlyObservableCollection<ValidationError> errors = Validation.GetErrors(this);
            //if (errors.Count > 0)
            //{
            //    ValidationError firstError = errors[0];
            //    firstError.ErrorContent.
            //}
            //else
            //{
                ApplyChanges = true;
                Close();
            //}
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ApplyChanges = false;
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            CrawlingRule rule = (CrawlingRule)DataContext;

            rule.Conditions.Add(new CrawlingCondition
            {
                Guid = Guid.NewGuid(),
                ComparisonType = CrawlingConditionComparisonType.Contains,
                FieldType = CrawlingConditionFieldType.Host,
                Value = ""
            });
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            CrawlingRule rule = (CrawlingRule)DataContext;
            CrawlingCondition condition = (CrawlingCondition)((Control)sender).DataContext;

            rule.Conditions.Remove(condition);
        }

        private void BtnMoveUp_CLick(object sender, RoutedEventArgs e)
        {
            CrawlingRule rule = (CrawlingRule)DataContext;
            CrawlingCondition condition = (CrawlingCondition)((Control)sender).DataContext;
            int index = rule.Conditions.IndexOf(condition);

            if (index > 0)
                rule.Conditions.Move(index, index - 1);
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            CrawlingRule rule = (CrawlingRule)DataContext;
            CrawlingCondition condition = (CrawlingCondition)((Control)sender).DataContext;
            int index = rule.Conditions.IndexOf(condition);

            if (index < rule.Conditions.Count - 1)
                rule.Conditions.Move(index, index + 1);
        }
    }
}
