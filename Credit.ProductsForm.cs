// Credit, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// Credit.ProductsForm
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Credit;
using Npgsql;

public class ProductsForm : Form
{
	private DatabaseHelper dbHelper;

	private DataTable dataTable;

	private IContainer components = null;

	private DataGridView dataGridViewProducts;

	private Button btnUpdate;

	private Button btnDelete;

	private TextBox txtProductName;

	private TextBox txtDescription;

	private TextBox txtCategory;

	private Button btnAdd;

	private Label label1;

	private Label label2;

	private Label label3;

	private Label Цена;

	private TextBox txtPrice;

	public ProductsForm()
	{
		InitializeComponent();
		dbHelper = new DatabaseHelper();
		LoadData();
		SetupForm();
		btnAdd.Click += btnAdd_Click;
		btnUpdate.Click += btnUpdate_Click;
		btnDelete.Click += btnDelete_Click;
		dataGridViewProducts.SelectionChanged += dataGridViewProducts_SelectionChanged;
	}

	private void SetupForm()
	{
		dataGridViewProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
		dataGridViewProducts.ReadOnly = true;
		dataGridViewProducts.MultiSelect = false;
		dataGridViewProducts.AllowUserToAddRows = false;
		dataGridViewProducts.AllowUserToDeleteRows = false;
		btnUpdate.Enabled = false;
		btnDelete.Enabled = false;
	}

	private void LoadData()
	{
		try
		{
			string query = "SELECT * FROM Products ORDER BY product_id";
			dataTable = dbHelper.ExecuteQuery(query);
			dataGridViewProducts.DataSource = dataTable;
			if (dataGridViewProducts.Columns.Contains("name"))
			{
				dataGridViewProducts.Columns["name"].HeaderText = "Название";
			}
			if (dataGridViewProducts.Columns.Contains("description"))
			{
				dataGridViewProducts.Columns["description"].HeaderText = "Описание";
			}
			if (dataGridViewProducts.Columns.Contains("category"))
			{
				dataGridViewProducts.Columns["category"].HeaderText = "Категория";
			}
			if (dataGridViewProducts.Columns.Contains("price"))
			{
				dataGridViewProducts.Columns["price"].HeaderText = "Цена";
			}
			if (dataGridViewProducts.Columns.Contains("product_id"))
			{
				dataGridViewProducts.Columns["product_id"].Visible = false;
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
				decimal price = ParsePrice(txtPrice.Text);
				string query = "INSERT INTO Products (name, description, category, price) \r\n                         VALUES (@name, @description, @category, @price)";
				NpgsqlParameter[] parameters = new NpgsqlParameter[4]
				{
					new NpgsqlParameter("@name", txtProductName.Text.Trim()),
					new NpgsqlParameter("@description", ((object)txtDescription.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@category", ((object)txtCategory.Text.Trim()) ?? ((object)DBNull.Value)),
					new NpgsqlParameter("@price", price)
				};
				int result = dbHelper.ExecuteNonQuery(query, parameters);
				if (result > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Товар добавлен успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось добавить товар!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Ошибка при добавлении товара: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnUpdate_Click(object sender, EventArgs e)
	{
		if (dataGridViewProducts.CurrentRow != null)
		{
			try
			{
				if (ValidateRequiredFields())
				{
					int productId = Convert.ToInt32(dataGridViewProducts.CurrentRow.Cells["product_id"].Value);
					decimal price = ParsePrice(txtPrice.Text);
					string query = "UPDATE Products SET \r\n                            name=@name, description=@description, \r\n                            category=@category, price=@price \r\n                            WHERE product_id=@productId";
					NpgsqlParameter[] parameters = new NpgsqlParameter[5]
					{
						new NpgsqlParameter("@name", txtProductName.Text.Trim()),
						new NpgsqlParameter("@description", ((object)txtDescription.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@category", ((object)txtCategory.Text.Trim()) ?? ((object)DBNull.Value)),
						new NpgsqlParameter("@price", price),
						new NpgsqlParameter("@productId", productId)
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
		MessageBox.Show("Выберите товар для обновления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void btnDelete_Click(object sender, EventArgs e)
	{
		if (dataGridViewProducts.CurrentRow != null)
		{
			DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранный товар?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result != DialogResult.Yes)
			{
				return;
			}
			try
			{
				int productId = Convert.ToInt32(dataGridViewProducts.CurrentRow.Cells["product_id"].Value);
				string query = "DELETE FROM Products WHERE product_id=@productId";
				NpgsqlParameter[] parameters = new NpgsqlParameter[1]
				{
					new NpgsqlParameter("@productId", productId)
				};
				int rowsAffected = dbHelper.ExecuteNonQuery(query, parameters);
				if (rowsAffected > 0)
				{
					LoadData();
					ClearFields();
					MessageBox.Show("Товар удален успешно!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
				else
				{
					MessageBox.Show("Не удалось удалить товар!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Ошибка при удалении товара: " + ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MessageBox.Show("Выберите товар для удаления!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
	}

	private void dataGridViewProducts_SelectionChanged(object sender, EventArgs e)
	{
		bool hasSelection = dataGridViewProducts.CurrentRow != null && dataGridViewProducts.CurrentRow.Index >= 0;
		btnUpdate.Enabled = hasSelection;
		btnDelete.Enabled = hasSelection;
		if (hasSelection)
		{
			DataGridViewRow row = dataGridViewProducts.CurrentRow;
			txtProductName.Text = row.Cells["name"].Value?.ToString() ?? "";
			txtDescription.Text = row.Cells["description"].Value?.ToString() ?? "";
			txtCategory.Text = row.Cells["category"].Value?.ToString() ?? "";
			if (row.Cells["price"].Value != null && row.Cells["price"].Value != DBNull.Value)
			{
				decimal price = Convert.ToDecimal(row.Cells["price"].Value);
				txtPrice.Text = price.ToString("0.00", CultureInfo.InvariantCulture);
			}
			else
			{
				txtPrice.Text = "";
			}
		}
	}

	private void ClearFields()
	{
		txtProductName.Clear();
		txtDescription.Clear();
		txtCategory.Clear();
		txtPrice.Clear();
		dataGridViewProducts.ClearSelection();
	}

	private bool ValidateRequiredFields()
	{
		if (string.IsNullOrWhiteSpace(txtProductName.Text))
		{
			MessageBox.Show("Название товара обязательно для заполнения!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtProductName.Focus();
			return false;
		}
		if (string.IsNullOrWhiteSpace(txtPrice.Text))
		{
			MessageBox.Show("Цена должна быть указана!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtPrice.Focus();
			return false;
		}
		string priceText = txtPrice.Text.Replace(',', '.');
		if (!decimal.TryParse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
		{
			MessageBox.Show("Цена должна быть числом! Используйте точку или запятую.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtPrice.Focus();
			return false;
		}
		if (price <= 0m)
		{
			MessageBox.Show("Цена должна быть положительным числом!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			txtPrice.Focus();
			return false;
		}
		return true;
	}

	private decimal ParsePrice(string priceText)
	{
		priceText = priceText.Replace(',', '.');
		return decimal.Parse(priceText, NumberStyles.Any, CultureInfo.InvariantCulture);
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
		this.dataGridViewProducts = new System.Windows.Forms.DataGridView();
		this.btnUpdate = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.txtProductName = new System.Windows.Forms.TextBox();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.txtCategory = new System.Windows.Forms.TextBox();
		this.btnAdd = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.Цена = new System.Windows.Forms.Label();
		this.txtPrice = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.dataGridViewProducts).BeginInit();
		base.SuspendLayout();
		this.dataGridViewProducts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGridViewProducts.Location = new System.Drawing.Point(1, 84);
		this.dataGridViewProducts.Name = "dataGridViewProducts";
		this.dataGridViewProducts.Size = new System.Drawing.Size(802, 311);
		this.dataGridViewProducts.TabIndex = 0;
		this.btnUpdate.Location = new System.Drawing.Point(588, 401);
		this.btnUpdate.Name = "btnUpdate";
		this.btnUpdate.Size = new System.Drawing.Size(96, 48);
		this.btnUpdate.TabIndex = 1;
		this.btnUpdate.Text = "Обновить";
		this.btnUpdate.UseVisualStyleBackColor = true;
		this.btnDelete.Location = new System.Drawing.Point(692, 401);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(96, 48);
		this.btnDelete.TabIndex = 1;
		this.btnDelete.Text = "Удалить";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.txtProductName.Location = new System.Drawing.Point(1, 55);
		this.txtProductName.Name = "txtProductName";
		this.txtProductName.Size = new System.Drawing.Size(100, 23);
		this.txtProductName.TabIndex = 2;
		this.txtDescription.Location = new System.Drawing.Point(107, 55);
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(100, 23);
		this.txtDescription.TabIndex = 2;
		this.txtCategory.Location = new System.Drawing.Point(213, 55);
		this.txtCategory.Name = "txtCategory";
		this.txtCategory.Size = new System.Drawing.Size(100, 23);
		this.txtCategory.TabIndex = 2;
		this.btnAdd.Location = new System.Drawing.Point(457, 55);
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(75, 23);
		this.btnAdd.TabIndex = 4;
		this.btnAdd.Text = "Добавить";
		this.btnAdd.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(22, 27);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(59, 15);
		this.label1.TabIndex = 5;
		this.label1.Text = "Название";
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(132, 27);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(53, 15);
		this.label2.TabIndex = 5;
		this.label2.Text = "Продукт";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(229, 27);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(64, 15);
		this.label3.TabIndex = 6;
		this.label3.Text = "Катигория";
		this.Цена.AutoSize = true;
		this.Цена.Location = new System.Drawing.Point(356, 27);
		this.Цена.Name = "Цена";
		this.Цена.Size = new System.Drawing.Size(35, 15);
		this.Цена.TabIndex = 7;
		this.Цена.Text = "Цена";
		this.txtPrice.Location = new System.Drawing.Point(319, 55);
		this.txtPrice.Name = "txtPrice";
		this.txtPrice.Size = new System.Drawing.Size(100, 23);
		this.txtPrice.TabIndex = 8;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(800, 450);
		base.Controls.Add(this.txtPrice);
		base.Controls.Add(this.Цена);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.btnAdd);
		base.Controls.Add(this.txtCategory);
		base.Controls.Add(this.txtDescription);
		base.Controls.Add(this.txtProductName);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnUpdate);
		base.Controls.Add(this.dataGridViewProducts);
		base.Name = "ProductsForm";
		this.Text = "Products";
		((System.ComponentModel.ISupportInitialize)this.dataGridViewProducts).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
