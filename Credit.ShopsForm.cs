// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.ShopsForm
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class ShopsForm : Form
{
	private DatabaseHelper dbHelper;

	private DataTable dataTable;

	private IContainer components = null;

	private DataGridView dataGridViewShops;

	private Button btnUpdate;

	private Button btnDelete;

	private TextBox txtShopName;

	private TextBox txtShopAddress;

	private TextBox txtShopPhone;

	private Label label1;

	private Label label2;

	private Label label3;

	private Button btnAdd;

	public ShopsForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadData();
		SetupForm();
		btnAdd.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		dataGridViewShops.SelectionChanged += dataGridViewShops_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewShops.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewShops.ReadOnly = true;
		dataGridViewShops.MultiSelect = false;
		dataGridViewShops.AllowUserToAddRows = false;
		dataGridViewShops.AllowUserToDeleteRows = false;
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT * FROM Shops ORDER BY shop_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewShops.DataSource = dataTable;
			if (dataGridViewShops.Columns.Contains("name"))
			{
				dataGridViewShops.Columns["name"].HeaderText = "Название";
			}
			if (dataGridViewShops.Columns.Contains("address"))
			{
				dataGridViewShops.Columns["address"].HeaderText = "Адрес";
			}
			if (dataGridViewShops.Columns.Contains("phone_number"))
			{
				dataGridViewShops.Columns["phone_number"].HeaderText = "Телефон";
			}
			if (dataGridViewShops.Columns.Contains("shop_id"))
			{
				dataGridViewShops.Columns["shop_id"].Visible = false;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnAdd_Click(object sender, EventArgs e)
	{
		try
		{
			if (ValidateRequiredFields())
			{
				string query = "INSERT INTO Shops (name, address, phone_number) \r\n                         VALUES (@name, @address, @phone)";
				NpgsqlParameter[] parameters = new NpgsqlParameter[3]
				{
					new NpgsqlParameter("@name", txtShopName.Text.Trim()),
					new NpgsqlParameter("@address", txtShopAddress.Text.Trim()),
					new NpgsqlParameter("@phone", ((object)txtShopPhone.Text.Trim()) ?? ((object)DBNull.Value))
				};
				int result = dbHelper.ExecuteNonQuery(query, parameters);
				if (result > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Магазин добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось добавить магазин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении магазина: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewShops.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int shopId = Convert.ToInt32(dataGridViewShops.CurrentRow.Cells["shop_id"].Value);
					string query = "UPDATE Shops SET \r\n                            name=@name, address=@address, phone_number=@phone \r\n                            WHERE shop_id=@shopId";
					NpgsqlParameter[] parameters = new NpgsqlParameter[4]
					{
						new NpgsqlParameter("@name", txtShopName.Text.Trim()),
						new NpgsqlParameter("@address", txtShopAddress.Text.Trim()),
						new NpgsqlParameter("@phone", ((object)txtShopPhone.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@shopId", shopId)
					};
					int result = dbHelper.ExecuteNonQuery(query, parameters);
					if (result > 0)
					{
						LoadData();
						MessageBox.Show("Данные обновлены успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
					}
					else
					{
						MessageBox.Show("Не удалось обновить данные!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					}
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при обновлении данных: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите магазин для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewShops.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранный магазин?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int shopId = Convert.ToInt32(dataGridViewShops.CurrentRow.Cells["shop_id"].Value);
				string query = "DELETE FROM Shops WHERE shop_id=@shopId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@shopId", shopId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Магазин удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить магазин!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении магазина: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите магазин для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void dataGridViewShops_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewShops.CurrentRow != null && dataGridViewShops.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (hasSelection)
		{
			DataGridViewRow row = dataGridViewShops.CurrentRow;
			txtShopName.Text = row.Cells["name"].Value?.ToString() ?? "";
			txtShopAddress.Text = row.Cells["address"].Value?.ToString() ?? "";
			txtShopPhone.Text = row.Cells["phone_number"].Value?.ToString() ?? "";
		}
	}

	private void ClearFields()
	{
		txtShopName.Clear();
		txtShopAddress.Clear();
		txtShopPhone.Clear();
		dataGridViewShops.ClearSelection();
	}

	private bool ValidateRequiredFields()
	{
		if (string.IsNullOrWhiteSpace(txtShopName.Text))
		{
			MessageBox.Show("Название магазина обязательно для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtShopName.Focus();
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtShopAddress.Text))
		{
			MessageBox.Show("Адрес магазина обязателен для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtShopAddress.Focus();
			return false;
		}
		return true;
	}

	private void btnClear_Click(object sender, EventArgs e)
	{
		ClearFields();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.dataGridViewShops = new System.Windows.Forms.DataGridView();
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.txtShopName = new System.Windows.Forms.TextBox();
		this.txtShopAddress = new System.Windows.Forms.TextBox();
		this.txtShopPhone = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.btnAdd = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewShops).BeginInit();
		base.SuspendLayout();
		this.dataGridViewShops.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewShops.Location = new System.Drawing.Point(3, 75);
		this.dataGridViewShops.Name = "dataGridViewShops";
		this.dataGridViewShops.Size = new System.Drawing.Size(804, 296);
		this.dataGridViewShops.TabIndex = 0;
		this.btnUpdate.Location = new System.Drawing.Point(603, 377);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(90, 46);
		this.btnUpdate.TabIndex = 1;
		this.btnUpdate.Text = "Обновить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(699, 377);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(89, 46);
		this.btnDelete.TabIndex = 2;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.txtShopName.Location = new System.Drawing.Point(3, 35);
		this.txtShopName.Name = "txtShopName";
		this.txtShopName.Size = new System.Drawing.Size(113, 23);
		this.txtShopName.TabIndex = 3;
		this.txtShopAddress.Location = new System.Drawing.Point(122, 35);
		this.txtShopAddress.Name = "txtShopAddress";
		this.txtShopAddress.Size = new System.Drawing.Size(109, 23);
		this.txtShopAddress.TabIndex = 3;
		this.txtShopPhone.Location = new System.Drawing.Point(237, 35);
		this.txtShopPhone.Name = "txtShopPhone";
		this.txtShopPhone.Size = new System.Drawing.Size(109, 23);
		this.txtShopPhone.TabIndex = 3;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(3, 9);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(113, 15);
		this.label1.TabIndex = 4;
		this.label1.Text = "Название магазина";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(148, 9);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(46, 15);
		this.label2.TabIndex = 4;
		this.label2.Text = "Адресс";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(258, 9);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(56, 15);
		this.label3.TabIndex = 4;
		this.label3.Text = "Телефон";
		this.btnAdd.Location = new System.Drawing.Point(361, 34);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(75, 23);
		this.btnAdd.TabIndex = 5;
		this.btnAdd.Text = "Добавить";
		this.btnAdd.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtShopPhone);
		base.Controls.Add(this.txtShopAddress);
		base.Controls.Add(this.txtShopName);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Controls.Add(this.dataGridViewShops);
		base.Name = "ShopsForm";
		this.Text = "Shops";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewShops).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
