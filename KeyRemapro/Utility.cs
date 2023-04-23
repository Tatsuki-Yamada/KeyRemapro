using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyRemapro
{
    public static class Utility
    {
        public static void ShowOnlyOneForm(Type formType)
        {
            Form targetForm = Application.OpenForms[formType.Name];
            if (targetForm == null)
            {
                Form createdForm = Activator.CreateInstance(formType) as Form;
                createdForm.Show();
            }
            else
            {
                targetForm.Show();
            }
        }
    }
}
