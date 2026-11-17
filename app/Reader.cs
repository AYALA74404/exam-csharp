using System;
using System.Collections.Generic;
using System.Text;

namespace App
{
    enum Type
    {
        student,
        teacher,
        guess
    }
    internal class Reader
    {
        string Reader_ID;
        public string Name { get; set; }
        public int Borrow_day { get; set; }

        public Type type { get; set; }

        public int Return_day { get; set; }

        public Reader(string reader_ID, string name, Type type, int borrow_day, int return_day)
        {
            Reader_ID = reader_ID;
            Name = name;
            this.type = type;
            Borrow_day = borrow_day;
            Return_day = return_day;
        }
        public Reader(string reader_ID, string name, Type type, int borrow_day)
        {
            Reader_ID = reader_ID;
            Name = name;
            this.type = type;
            Borrow_day = borrow_day;
        }

        //בעיקרון להעביר למחלקת REPORT!!!
        public static int calculatorForStudents(Reader reader)
        {
            int sumDaysOfBorrow = 0;
            if (reader.Return_day != 0 && reader.Borrow_day != 0)
            {
                sumDaysOfBorrow = reader.Return_day - reader.Borrow_day;
            }
            // if (sumDaysOfBorrow < 16)//חישוב כולל יום חסד
            if (sumDaysOfBorrow < 15)
                return 0;
            else  
                return sumDaysOfBorrow - 14;
            
            
        }


        public static int NeedToPay(Reader reader)
        {
            int sumToPay = 0;
            if (reader == null)
                return 0;
            if (reader.type == Type.student)
                sumToPay = calculatorForStudents(reader);
                 
            return sumToPay;
        }

    }
}
