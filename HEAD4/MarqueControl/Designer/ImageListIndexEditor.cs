////////////////////////////////////////////////////////
//Author : Manish Ranjan Kumar
////////////////////////////////////////////////////////
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using MarqueControl.Attributes;

namespace MarqueControl.Designer
{
    internal class ImageListIndexEditor : UITypeEditor
    {
        protected ImageList currentImageList;
        protected PropertyDescriptor currentImageListProp;
        protected object currentInstance;
        protected UITypeEditor imageEditor = ((UITypeEditor)TypeDescriptor.GetEditor(typeof(Image), typeof(UITypeEditor)));
        private ImageIndexUI imageUI;

        internal UITypeEditor ImageEditor
        {
            get { return imageEditor; }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return value;
                }
                if (imageUI == null)
                {
                    imageUI = new ImageIndexUI();
                }
                InitializeImageList(context);
                imageUI.Start(edSvc, value, currentImageList);
                edSvc.DropDownControl(imageUI);
                value = imageUI.Value;
                imageUI.End();
            }
            return value;
        }

        private void InitializeImageList(ITypeDescriptorContext context)
        {
            object instance = context.Instance;
            PropertyDescriptor imageListProperty = GetImageListProperty(context.PropertyDescriptor, ref instance);
            while ((instance != null) && (imageListProperty == null))
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(instance);
                foreach (PropertyDescriptor descriptor2 in properties)
                {
                    if (typeof(ImageList).IsAssignableFrom(descriptor2.PropertyType))
                    {
                        imageListProperty = descriptor2;
                        break;
                    }
                }
            }
            if (imageListProperty != null)
            {
                currentImageList = (ImageList)imageListProperty.GetValue(instance);
                currentImageListProp = imageListProperty;
                currentInstance = instance;
            }
            else
            {
                currentImageList = null;
                currentImageListProp = imageListProperty;
                currentInstance = instance;
            }
        }

        protected virtual Image GetImage(ITypeDescriptorContext context, int index, string key, bool useIntIndex)
        {
            Image image = null;
            object instance = context.Instance;
            if (!(instance is object[]))
            {
                if ((index < 0) && (key == null))
                {
                    return image;
                }
                InitializeImageList(context);
                if (currentImageList != null)
                {
                    if (useIntIndex)
                    {
                        if ((currentImageList != null) && (index < currentImageList.Images.Count))
                        {
                            index = (index > 0) ? index : 0;
                            image = currentImageList.Images[index];
                        }
                        return image;
                    }
                    return currentImageList.Images[key];
                }
            }
            return null;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return ((imageEditor != null) && imageEditor.GetPaintValueSupported(context));
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (ImageEditor != null)
            {
                Image image = null;
                if (e.Value is int)
                {
                    image = GetImage(e.Context, (int)e.Value, null, true);
                }
                else if (e.Value is string)
                {
                    image = GetImage(e.Context, -1, (string)e.Value, false);
                }
                if (image != null)
                {
                    ImageEditor.PaintValue(new PaintValueEventArgs(e.Context, image, e.Graphics, e.Bounds));
                }
            }
        }

        public static PropertyDescriptor GetImageListProperty(PropertyDescriptor currentComponent, ref object instance)
        {
            if (instance is object[])
            {
                return null;
            }
            PropertyDescriptor descriptor = null;
            object component = instance;
            ImagePropertyAttribute attribute = currentComponent.Attributes[typeof(ImagePropertyAttribute)] as ImagePropertyAttribute;
            if (attribute != null)
            {
                string[] strArray = attribute.PropertyName.Split(new char[] { '.' });
                for (int i = 0; i < strArray.Length; i++)
                {
                    if (component == null)
                    {
                        return descriptor;
                    }
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)[strArray[i]];
                    if (descriptor2 == null)
                    {
                        return descriptor;
                    }
                    if (i == (strArray.Length - 1))
                    {
                        if (typeof(ImageList).IsAssignableFrom(descriptor2.PropertyType))
                        {
                            instance = component;
                            return descriptor2;
                        }
                    }
                    else
                    {
                        component = descriptor2.GetValue(component);
                    }
                }
            }
            return descriptor;
        }

        #region Nested type: ImageIndexUI

        private class ImageIndexUI : ListBox
        {
            private IWindowsFormsEditorService edSvc;
            private int value = -1;

            public ImageIndexUI()
            {
#pragma warning disable DoNotCallOverridableMethodsInConstructor
                ItemHeight = 20;
                Height = 20 * 5;
                DrawMode = DrawMode.OwnerDrawFixed;
                Dock = DockStyle.Fill;
#pragma warning restore DoNotCallOverridableMethodsInConstructor
                BorderStyle = BorderStyle.None;
            }

            public int Value
            {
                get { return value; }
            }

            public void End()
            {
                edSvc = null;
                value = -1;
            }

            protected override void OnClick(EventArgs e)
            {
                base.OnClick(e);
                value = SelectedIndex - 1;
                edSvc.CloseDropDown();
            }

            protected override void OnDrawItem(DrawItemEventArgs die)
            {
                base.OnDrawItem(die);
                if (die.Index != -1)
                {
                    Bitmap image = Items[die.Index] as Bitmap;
                    string s = (die.Index - 1).ToString();
                    Font font = die.Font;
                    Brush brush = new SolidBrush(die.ForeColor);
                    die.DrawBackground();
                    if (image != null)
                    {
                        die.Graphics.DrawRectangle(SystemPens.WindowText, new Rectangle(die.Bounds.X, die.Bounds.Y, 18, 18));
                        die.Graphics.DrawImage(image, new Rectangle(die.Bounds.X + 2, die.Bounds.Y + 2, 16, 16));
                        die.Graphics.DrawString(s, font, brush, die.Bounds.X + 36, die.Bounds.Y + ((die.Bounds.Height - font.Height) / 2));
                    }
                    else
                    {
                        die.Graphics.DrawString("(none)", font, brush, die.Bounds.X + 36, die.Bounds.Y + ((die.Bounds.Height - font.Height) / 2));
                    }
                    brush.Dispose();
                }
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if (((keyData & Keys.KeyCode) == Keys.Return) && ((keyData & (Keys.Alt | Keys.Control)) == Keys.None))
                {
                    OnClick(EventArgs.Empty);
                    return true;
                }
                return base.ProcessDialogKey(keyData);
            }

            public void Start(IWindowsFormsEditorService service, object objectValue, ImageList list)
            {
                edSvc = service;
                value = (int)objectValue;
                Items.Clear();
                Items.Add("(none");
                if (list != null)
                {
                    for (int i = 0; i < list.Images.Count; i++)
                    {
                        Items.Add(list.Images[i]);
                    }
                }

                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == objectValue)
                    {
                        SelectedIndex = i + 1;
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
