using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
	public class FormLichSuNhapQua : Form
	{
		private DataGridView dgv;

		private TextBox txtTim;


		public FormLichSuNhapQua()
		{
			InitUI();

			LoadData();
		}


		private void InitUI()
		{
			this.Text =
				"📋 LỊCH SỬ NHẬP QUÀ TẶNG";

			this.Size =
				new Size(1050, 600);

			this.StartPosition =
				FormStartPosition.CenterParent;

			this.BackColor =
				Color.FromArgb(245, 247, 250);

			this.Font =
				new Font("Segoe UI", 9.5F);


			Panel pnlTop =
				new Panel
				{
					Dock =
						DockStyle.Top,

					Height =
						60,

					Padding =
						new Padding(15)
				};


			Label lbl =
				new Label
				{
					Text =
						"Tìm mã quà / tên quà / nguồn cấp:",

					Location =
						new Point(15, 18),

					AutoSize =
						true,

					Font =
						new Font(
							"Segoe UI",
							9.5F,
							FontStyle.Bold
						)
				};


			txtTim =
				new TextBox
				{
					Location =
						new Point(245, 14),

					Width =
						320
				};


			txtTim.TextChanged +=
				(s, e) =>
				LoadData();


			Button btnXoa =
				new Button
				{
					Text =
						"✖ Xóa",

					Location =
						new Point(575, 12),

					Width =
						80,

					Height =
						30,

					BackColor =
						Color.FromArgb(
							108,
							117,
							125
						),

					ForeColor =
						Color.White,

					FlatStyle =
						FlatStyle.Flat
				};


			btnXoa.Click +=
				(s, e) =>
				txtTim.Clear();


			pnlTop.Controls.AddRange(
				new Control[]
				{
					lbl,
					txtTim,
					btnXoa
				}
			);


			dgv =
				new DataGridView
				{
					Dock =
						DockStyle.Fill,

					ReadOnly =
						true,

					AllowUserToAddRows =
						false,

					AllowUserToDeleteRows =
						false,

					AutoSizeColumnsMode =
						DataGridViewAutoSizeColumnsMode.Fill,

					RowHeadersVisible =
						false,

					SelectionMode =
						DataGridViewSelectionMode.FullRowSelect,

					BackgroundColor =
						Color.White
				};


			dgv.EnableHeadersVisualStyles =
				false;

			dgv.ColumnHeadersDefaultCellStyle.BackColor =
				Color.FromArgb(24, 43, 73);

			dgv.ColumnHeadersDefaultCellStyle.ForeColor =
				Color.White;

			dgv.ColumnHeadersDefaultCellStyle.Font =
				new Font(
					"Segoe UI",
					9F,
					FontStyle.Bold
				);


			dgv.CellFormatting +=
				Dgv_CellFormatting;


			Panel pnlBottom =
				new Panel
				{
					Dock =
						DockStyle.Bottom,

					Height =
						50,

					Padding =
						new Padding(10)
				};


			Button btnDong =
				new Button
				{
					Text =
						"✖ Đóng",

					Dock =
						DockStyle.Right,

					Width =
						100,

					BackColor =
						Color.FromArgb(
							108,
							117,
							125
						),

					ForeColor =
						Color.White,

					FlatStyle =
						FlatStyle.Flat
				};


			btnDong.Click +=
				(s, e) =>
				this.Close();


			pnlBottom.Controls.Add(
				btnDong
			);


			this.Controls.Add(dgv);
			this.Controls.Add(pnlBottom);
			this.Controls.Add(pnlTop);
		}


		private void LoadData()
		{
			string kw =
				txtTim?
				.Text?
				.Trim()
				.ToLower()
				?? "";


			var ds =
				QuanLyKhoQuaTangData
					.DanhSachNhapQua
					.Where(x =>
						x != null &&
						(
							string.IsNullOrEmpty(kw)

							||

							(x.MaQua ?? "")
								.ToLower()
								.Contains(kw)

							||

							(x.TenQua ?? "")
								.ToLower()
								.Contains(kw)

							||

							(x.NguonCap ?? "")
								.ToLower()
								.Contains(kw)
						)
					)
					.OrderByDescending(
						x => x.NgayNhap
					)
					.Select(x => new
					{
						Mã_Phiếu =
							x.MaPhieu,

						Mã_Quà =
							x.MaQua,

						Tên_Quà =
							x.TenQua,

						Nguồn_Cấp =
							x.NguonCap,

						Ngày_Nhập =
							x.NgayNhap
								.ToString(
									"dd/MM/yyyy HH:mm"
								),

						SL_Nhập =
							x.SoLuongNhap,

						HSD =
							x.HanSuDung.HasValue
							? x.HanSuDung.Value.ToString(
								"dd/MM/yyyy"
							)
							: "Không áp dụng",

						Trạng_Thái =
							x.TrangThaiHSD,

						Ghi_Chú =
							x.GhiChu
					})
					.ToList();


			dgv.DataSource = null;

			dgv.DataSource = ds;
		}


		private void Dgv_CellFormatting(
			object sender,
			DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0)
				return;


			DataGridViewRow row =
				dgv.Rows[e.RowIndex];


			if (!dgv.Columns.Contains(
				"Trạng_Thái"))
				return;


			string status =
				row.Cells["Trạng_Thái"]
					.Value?
					.ToString()
				?? "";


			if (status.Contains(
				"Đã hết hạn"))
			{
				row.DefaultCellStyle.BackColor =
					Color.FromArgb(
						255,
						220,
						220
					);

				row.DefaultCellStyle.ForeColor =
					Color.DarkRed;
			}
			else if (status.Contains("⚠"))
			{
				row.DefaultCellStyle.BackColor =
					Color.FromArgb(
						255,
						243,
						205
					);
			}
		}
	}
}