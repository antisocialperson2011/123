using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VotingApp
{
    public class Form1 : Form
    {
        private readonly Dictionary<string, int> votes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> voters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private bool active;

        private GroupBox grpSetup;
        private TextBox txtTopic;
        private TextBox txtOptions;
        private Button btnStart;
        private Button btnEnd;

        private GroupBox grpVote;
        private Label lblCurrentTopic;
        private TextBox txtVoter;
        private ListBox lstOptions;
        private Button btnVote;

        private GroupBox grpResults;
        private Label lblCount;
        private ListBox lstResults;
        private Label lblWinner;

        public Form1()
        {
            InitializeUi();
            UpdateState();
        }

        private void InitializeUi()
        {
            Text = "Голосование";
            ClientSize = new Size(700, 520);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f);

            grpSetup = new GroupBox { Text = "Настройка голосования", Location = new Point(10, 10), Size = new Size(330, 300) };
            grpSetup.Controls.Add(new Label { Text = "Тема:", Location = new Point(10, 25), AutoSize = true });
            txtTopic = new TextBox { Location = new Point(10, 45), Width = 305 };
            grpSetup.Controls.Add(txtTopic);
            grpSetup.Controls.Add(new Label { Text = "Варианты (каждый с новой строки):", Location = new Point(10, 80), AutoSize = true });
            txtOptions = new TextBox { Location = new Point(10, 100), Size = new Size(305, 130), Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical };
            grpSetup.Controls.Add(txtOptions);
            btnStart = new Button { Text = "Начать голосование", Location = new Point(10, 245), Size = new Size(150, 36) };
            btnStart.Click += BtnStart_Click;
            grpSetup.Controls.Add(btnStart);
            btnEnd = new Button { Text = "Закончить голосование", Location = new Point(165, 245), Size = new Size(150, 36) };
            btnEnd.Click += BtnEnd_Click;
            grpSetup.Controls.Add(btnEnd);

            grpVote = new GroupBox { Text = "Участие в голосовании", Location = new Point(350, 10), Size = new Size(340, 300) };
            lblCurrentTopic = new Label { Text = "Голосование не начато", Location = new Point(10, 25), Size = new Size(320, 40), Font = new Font(Font, FontStyle.Bold) };
            grpVote.Controls.Add(lblCurrentTopic);
            grpVote.Controls.Add(new Label { Text = "Ваше имя:", Location = new Point(10, 72), AutoSize = true });
            txtVoter = new TextBox { Location = new Point(10, 92), Width = 315 };
            grpVote.Controls.Add(txtVoter);
            grpVote.Controls.Add(new Label { Text = "Выберите вариант:", Location = new Point(10, 125), AutoSize = true });
            lstOptions = new ListBox { Location = new Point(10, 145), Size = new Size(315, 100) };
            grpVote.Controls.Add(lstOptions);
            btnVote = new Button { Text = "Проголосовать", Location = new Point(10, 252), Size = new Size(315, 36) };
            btnVote.Click += BtnVote_Click;
            grpVote.Controls.Add(btnVote);

            grpResults = new GroupBox { Text = "Результаты", Location = new Point(10, 320), Size = new Size(680, 190) };
            lblCount = new Label { Text = "Участников: 0", Location = new Point(10, 25), AutoSize = true };
            grpResults.Controls.Add(lblCount);
            lstResults = new ListBox { Location = new Point(10, 50), Size = new Size(660, 95) };
            grpResults.Controls.Add(lstResults);
            lblWinner = new Label { Text = "", Location = new Point(10, 152), Size = new Size(660, 30), Font = new Font(Font.FontFamily, 11f, FontStyle.Bold), ForeColor = Color.DarkGreen };
            grpResults.Controls.Add(lblWinner);

            Controls.Add(grpSetup);
            Controls.Add(grpVote);
            Controls.Add(grpResults);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            string topic = txtTopic.Text.Trim();
            if (topic.Length == 0)
            {
                MessageBox.Show("Введите тему голосования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var options = txtOptions.Lines
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (options.Count < 2)
            {
                MessageBox.Show("Нужно минимум два разных варианта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            votes.Clear();
            voters.Clear();
            foreach (var option in options)
                votes[option] = 0;

            lstOptions.Items.Clear();
            foreach (var option in options)
                lstOptions.Items.Add(option);

            lblCurrentTopic.Text = topic;
            lblWinner.Text = "";
            txtVoter.Clear();
            active = true;
            RefreshResults();
            UpdateState();
        }

        private void BtnVote_Click(object sender, EventArgs e)
        {
            if (!active)
                return;

            string name = txtVoter.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Введите имя.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (lstOptions.SelectedItem == null)
            {
                MessageBox.Show("Выберите вариант.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (voters.Contains(name))
            {
                MessageBox.Show("Этот участник уже проголосовал.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string option = (string)lstOptions.SelectedItem;
            votes[option]++;
            voters.Add(name);

            txtVoter.Clear();
            lstOptions.ClearSelected();
            txtVoter.Focus();
            RefreshResults();
        }

        private void BtnEnd_Click(object sender, EventArgs e)
        {
            if (!active)
                return;

            active = false;
            RefreshResults();
            ShowWinner();
            UpdateState();
        }

        private void RefreshResults()
        {
            lblCount.Text = "Участников: " + voters.Count;
            lstResults.Items.Clear();
            foreach (var pair in votes.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
                lstResults.Items.Add(pair.Key + " — " + pair.Value);
        }

        private void ShowWinner()
        {
            if (voters.Count == 0)
            {
                lblWinner.Text = "Никто не проголосовал";
                return;
            }

            int max = votes.Values.Max();
            var winners = votes.Where(x => x.Value == max).Select(x => x.Key).ToList();

            if (winners.Count == 1)
                lblWinner.Text = "Победитель: " + winners[0] + " (" + max + ")";
            else
                lblWinner.Text = "Ничья: " + string.Join(", ", winners) + " (по " + max + ")";
        }

        private void UpdateState()
        {
            txtTopic.Enabled = !active;
            txtOptions.Enabled = !active;
            btnStart.Enabled = !active;
            btnEnd.Enabled = active;
            txtVoter.Enabled = active;
            lstOptions.Enabled = active;
            btnVote.Enabled = active;
            if (!active && votes.Count == 0)
                lblCurrentTopic.Text = "Голосование не начато";
        }
    }
}
