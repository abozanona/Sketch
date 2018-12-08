using Sketch.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Linq;
using System.ComponentModel;

namespace Sketch.Classes
{
    public class ExtendedDataGridViewComboBoxColumn : DataGridViewComboBoxColumn
    {
        public string propertyName { get; set; }
    }
    public class DataGridViewEvents<T> where T : BaseModel, new()
    {
        private BindingSource bindingSource = new BindingSource();
        DataGridView dgv;
        private string getDisplayName(string defaultName, object[] attrs)
        {
            foreach (object attr2 in attrs)
            {
                if (attr2 is DisplayNameAttribute displayName)
                {
                    return displayName.DisplayName;
                }
            }
            return defaultName;
        }
        private bool isDBColumnAttribute(object[] attrs)
        {
            foreach (object attr2 in attrs)
            {
                if (attr2 is DBColumnAttribute)
                {
                    return true;
                }
            }
            return false;
        }
        private DBColumnAttribute getDBColumnAttribute(object[] attrs)
        {
            foreach (object attr2 in attrs)
            {
                if (attr2 is DBColumnAttribute attr)
                {
                    return attr;
                }
            }
            return null;
        }
        public DataGridViewEvents(DataGridView control)
        {
            dgv = control;
            string HeaderText;
            var query = AppDatabase.database.Table<T>();
            foreach (var stock in query)
            {
                bindingSource.Add(stock);
            }

            dgv.AutoGenerateColumns = false;
            //dgv.AutoSize = true;
            dgv.DataSource = bindingSource;
            dgv.CellContentClick += dgvWorkTypes_CellContentClick;
            dgv.CellEndEdit += dgvWorkTypes_CellEndEdit;
            dgv.DataError += Dgv_DataError;
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (new List<string> {
                    "id",
                    "_id",
                    "_name"
                }.Contains(property.Name))
                {
                    continue;
                }
                object[] attrs = property.GetCustomAttributes(true);
                HeaderText = getDisplayName(property.Name, attrs);
                if (property.PropertyType.IsEnum)
                {
                    DataGridViewComboBoxColumn combo = new DataGridViewComboBoxColumn
                    {
                        DataSource = Enum.GetValues(property.PropertyType)
                            .Cast<Enum>()
                            .Select(value => new
                            {
                                (Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description,
                                value
                            })
                            .OrderBy(item => item.value)
                            .ToList(),
                        DataPropertyName = property.Name,
                        HeaderText = HeaderText,
                        DisplayMember = "Description",
                        ValueMember = "value"
                    };
                    dgv.Columns.Add(combo);
                }
                else if (property.PropertyType.IsPrimitive && isDBColumnAttribute(attrs))
                {
                    DBColumnAttribute tblName = getDBColumnAttribute(attrs);
                    HeaderText = getDisplayName(property.Name, attrs);

                    TableMapping map = new TableMapping(tblName.tableType);

                    //thanks to https://answers.unity.com/questions/841093/passing-type-variable-to-generic-type-variable-not.html
                    MethodInfo genericFunction = AppDatabase.database.GetType().GetMethod("Table");
                    MethodInfo realFunction = genericFunction.MakeGenericMethod(tblName.tableType);
                    dynamic query2 = realFunction.Invoke(AppDatabase.database, new object[] { });

                    List<BaseModel> items = new List<BaseModel>();
                    foreach (BaseModel stock in query2)
                    {
                        var x = Activator.CreateInstance(tblName.tableType);
                        x = stock;
                        items.Add((BaseModel)x);
                    }

                    DataGridViewComboBoxColumn combo = new ExtendedDataGridViewComboBoxColumn
                    {
                        DataSource = items,
                        propertyName = property.Name,
                        HeaderText = HeaderText,
                        DisplayMember = "_name",
                        ValueMember = "_id",
                        ValueType = typeof(int),
                        DataPropertyName = property.Name,
                    };
                    dgv.Columns.Add(combo);
                }
                else if (property.PropertyType.IsPrimitive)
                {
                    DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn
                    {
                        DataPropertyName = property.Name,
                        HeaderText = HeaderText,
                        ValueType = property.GetType(),
                    };
                    dgv.Columns.Add(column);
                }
                else if (property.PropertyType == typeof(string))
                {
                    DataGridViewTextBoxColumn combo = new DataGridViewTextBoxColumn
                    {
                        HeaderText = HeaderText,
                        ValueType = typeof(string),
                        DataPropertyName = property.Name,
                    };
                    dgv.Columns.Add(combo);
                }
                else if (property.PropertyType == typeof(DateTime))
                {
                    DataGridViewTextBoxColumn combo = new DataGridViewTextBoxColumn
                    {
                        HeaderText = HeaderText,
                        ValueType = typeof(DateTime),
                        DataPropertyName = property.Name,
                    };
                    combo.DefaultCellStyle.Format = "MM/dd/yyyy";
                    dgv.Columns.Add(combo);
                }
            }

            DataGridViewButtonColumn btnColumn = new DataGridViewButtonColumn
            {
                HeaderText = "حذف",
                Text = "حذف",
                UseColumnTextForButtonValue = true
            };
            dgv.Columns.Add(btnColumn);
        }

        private void Dgv_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private string RandomString(int length = 10)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return "V" + new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public void btnAdd_Click(object sender, EventArgs e)
        {
            T newObj = (T)new T().createEmpty();
            AppDatabase.database.Insert(newObj);
            bindingSource.Add(newObj);
        }

        public void dgvWorkTypes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn/* &&
                e.ColumnIndex == 0*/)
            {
                if (MessageBox.Show("هل أنت متأكد أنك تريد حذف هذا الصف؟", "تنبيه", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    AppDatabase.database.Delete((T)bindingSource[e.RowIndex]);
                    bindingSource.RemoveAt(e.RowIndex);
                }
            }
        }

        public void dgvWorkTypes_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv.Columns[e.ColumnIndex].GetType() == typeof(ExtendedDataGridViewComboBoxColumn))
            {
                ExtendedDataGridViewComboBoxColumn dgvcbc = (ExtendedDataGridViewComboBoxColumn)dgv.Columns[e.ColumnIndex];
                if (dgv.Rows[e.RowIndex].Cells[dgvcbc.Name].Value == null)
                {
                    return;
                }
            }
            AppDatabase.database.Update((T)bindingSource[e.RowIndex]);
        }
    }
}