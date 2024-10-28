using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundWorker_study
{
    public partial class frmTest : Form
    {
        private BackgroundWorker test_bw = new BackgroundWorker();
        private int numberToCompute = 0;
        private int highestPercentageReached = 0;

        public frmTest()
        {
            InitializeComponent();

            //스레드 작업 도중 취소 가능 여부
            test_bw.WorkerSupportsCancellation = true;
            //스레드 작업 진행상황 가능 여부
            test_bw.WorkerReportsProgress = true;

            InitializeBackgroundWorker();
        }

        #region BackgroundWorker Event
        private void InitializeBackgroundWorker()
        {
            //스레드가 run시에 호출되는 핸들러 등록
            test_bw.DoWork += new DoWorkEventHandler(test_bw_DoWork);

            //ReportProgress 메소드 호출 시 호출되는 핸들러 등록
            test_bw.ProgressChanged += new ProgressChangedEventHandler(test_bw_ProgressChanged);

            //스레드 완료(종료)시 호출되는 핸들러 등록
            test_bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(test_bw_RunWorkerCompleted);
        }

        //test_bw.DoWork
        private void test_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //결과값에 피보나치 결과 입력, DoworkEventArgs에 결과가 리턴됨
            //현재 결과는 test_bw_RunWorkerComplete에서 확인 가능
            e.Result = ComputeFibonacci((int)e.Argument, worker, e);
        }

        //test_bw.ProgressChanged
        private void test_bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Value = e.ProgressPercentage;
        }

        //test_bw.RunWorkerCompleted
        private void test_bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // 에러 처리
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled)   // 만약 중간에 종료가 된 것이라면 Canceled로 표시
            {
                lb_result.Text = "Canceled";
            }
            else
            {
                lb_result.Text = e.Result.ToString();   // 완료가 되었다면 결과값 입력
            }

            // 숫자 조정 활성화
            this.numericUpDown1.Enabled = true;

            // 시작 버튼 활성화
            btn_Start.Enabled = true;

            // 종료 버튼 비활성화
            btn_Stop.Enabled = false;
        }
        #endregion

        long ComputeFibonacci(int n, BackgroundWorker worker, DoWorkEventArgs e)
        {
            //파라미터는 0보다 크고 91보다 작아함, 91보다 클 경우 long이 오버플로우
            if ((n < 0) || (n > 91))
            {
                throw new ArgumentException(
                    "value must be >= 0 and <= 91", "n");
            }
            long result = 0;

            //유저가 취소 했다면 DoWorkEventArgs에 Cancel되었다고 입력
            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
            else
            {
                if (n < 2)
                {
                    result = 1;
                }
                else
                {
                    result = ComputeFibonacci(n - 1, worker, e) +
                             ComputeFibonacci(n - 2, worker, e);
                }

                //중간 처리 값을 지속적으로 보고
                int percentComplete = (int)((float)n / (float)numberToCompute * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
            }
            return result;
        }

        #region Object Event
        private void btn_Start_Click(object sender, EventArgs e)
        {
            lb_result.Text = String.Empty;

            this.numericUpDown1.Enabled = false;
            this.btn_Start.Enabled = false;
            this.btn_Stop.Enabled = true;

            numberToCompute = (int)numericUpDown1.Value;
            highestPercentageReached = 0;

            test_bw.RunWorkerAsync(numberToCompute);
        }

        private void btn_Stop_Click(object sender, EventArgs e)
        {
            //백그라운드 작업 중지
            this.test_bw.CancelAsync();

            btn_Stop.Enabled = false;
        }
        #endregion
    }
}