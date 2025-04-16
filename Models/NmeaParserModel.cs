using System;

namespace PolyGPS.Models
{
    public class NmeaParserModel
    {

        private Dictionary<string, string> _data = new Dictionary<string, string>();

        public void ReadSentence(string path)
        {
            string str;
            string[] sentence = new string[] {};
            StreamReader NMEAdata = File.OpenText(path);
            str = NMEAdata.ReadLine();
            str = str.Substring(1);
            sentence = str.Split(',');
            _data.Add("pos_sys", sentence[0].Substring(0, 2));
            _data.Add("sen_type", sentence[0].Substring(3, 5));
            if (_data["sen_type"] == "GGA") //GGA - информация о фиксированном решении
            {
                _data.Add("time", sentence[1]); //время
                _data.Add("width_value", sentence[2]); //значение широты
                _data.Add("width_hemisphere", sentence[3]);// полусфера - северная/южная
                _data.Add("longitude_value", sentence[4]);// значение долготы
                _data.Add("longitude_hemisphere", sentence[5]);// полусфера - западная/восточная
                _data.Add("sol_type", sentence[6]); // тип решения
                _data.Add("sat_amount", sentence[7]);// количество используемых спутников
                _data.Add("hdop", sentence[8]); // HDOP - геометрический фактор
                _data.Add("altitude", sentence[9]); // высота над уровнем моря
                _data.Add("WGS_altitude", sentence[11]); // высота над эипсоидом WGS 84
                if (_data["sol_type"] == "DGPS") // Проверка на включение DGPS решения
                {
                    _data.Add("last_mod", sentence[12]); // Время с получения последней DGPS поправки
                    _data.Add("base_st_id", sentence[13]); // Идентификационный номер базовой станции
                }
            }
            else if (_data["sen_type"] == "GSA") // GSA - общая инфа о спутниках
            {
                _data.Add("sol_des_type", sentence[1]); // Тип определения решения (А - автоматический, М - ручной)
                _data.Add("sol_type", sentence[2]); // Тип решения
                for(short i = 3; i < 15; i++) 
                {
                    _data.Add(i.ToString() + "_prn_code", sentence[i]); // 12 PRN кодов для определения позиций спутников
                }
                _data.Add("pdop", sentence[15]); // Пространственная геом поравка, PDOP
                _data.Add("hdop", sentence[16]); // горизонтальный геометрический фактор, HDOP
                _data.Add("vdop", sentence[17]); // вертикальный геометрический фактор, VDOP
            }
            else if (_data["sen_type"] == "GSV") // GSV - Детальная информация о спутниках
            {
                _data.Add("message_amount", sentence[1]); // количество сообщений GSV в пакете
                _data.Add("message_num", sentence[2]); //номер сообщения в пакете (от 1 до 3)
                _data.Add("sat_amount", sentence[3]); //количество видимых спутников
                for (int i = 0; i < (sentence.Length / 4) - 1; i++)
                {
                    _data.Add(i.ToString() + "_sat_num", sentence[4 + i]); // номер спутника
                    _data.Add(i.ToString() + "_elev_angle", sentence[5 + i]); // угол возвышения, в градусах
                    _data.Add(i.ToString() + "_azimuth", sentence[6 + i]); //азимут в градусах
                    _data.Add(i.ToString() + "_snr", sentence[7 + i]); // SNR, уровень сигнала (signal-to-noise ratio)
                }
            }
            else if (_data["sen_type"] == "RMC") // RMC - рекомендованный минимальный набор GPS данных
            {
                _data.Add("time", sentence[1]); //UTC время
                _data.Add("status", sentence[2]); //статус (А- активный, V- игнорировать)
                _data.Add("width_value", sentence[3]); //значение широты
                _data.Add("width_hemisphere", sentence[4]);// полусфера (северная/южная)
                _data.Add("longitude_value", sentence[5]);// значение долготы
                _data.Add("longitude_hemisphere", sentence[6]);// полусвера(западная/восточная)
                _data.Add("velocity", sentence[7]);//Скорость, в узлах
                _data.Add("angle", sentence[8]);//Скорость, в узлах
                _data.Add("date", sentence[9]);//Дата
                _data.Add("mag_var", sentence[10]);// Магнитные вариации
            }
            else if (_data["sen_type"] == "GLL")
            {
                _data.Add("width_value", sentence[1]); //значение широты
                _data.Add("width_hemisphere", sentence[2]);// полусфера - северная/южная
                _data.Add("longitude_value", sentence[3]);// значение долготы
                _data.Add("longitude_hemisphere", sentence[4]);// полусфера - западная/восточная
                _data.Add("time", sentence[5]); // время фиксации
                _data.Add("status", sentence[6]); //статус (А- активный, V- игнорировать)
            }
            else
            {
                _data["sen_type"] = "error";
            }
        }
        public string GetPosSysType() //возвращает тип позиционной системы (GPS, GLONASS, etc)
        {
            return _data["pos_sys"];
        }
        public string GetSenType() //возвращает тип предложения (GGA, GSA, etc)
        {
            return _data["sen_type"];
        }

        public int GetTime() // получение времени в целочисленном формате
        {
            try
            {
                return int.Parse(_data["time"]);
            }
            catch(ArgumentNullException e)
            {
                Console.WriteLine("No time specified in this sentence", e.Message);
                return 0;
            }
        }
        public float[] GetCoord() //Вовращает массив с координатам - [широта; долгота] >0 : Северное/Восточное полушарие; <0 :Южное/Западное полушарие
        {
            float[] coord = new float[2];
            try
            {
                if(_data["width_hemisphere"] == "N")
                {
                    coord[0] = float.Parse(_data["width_value"]) / 10;
                }
                else
                {
                    coord[0] = -1 * float.Parse(_data["width_value"]) / 10;
                }
                if (_data["longitude_hemisphere"] == "W")
                {
                    coord[1] = float.Parse(_data["longitude_value"]) / 10;
                }
                else
                {
                    coord[1] = -1 * float.Parse(_data["longitude_value"]) / 10;
                }
                
            }
            catch(ArgumentNullException e)
            {
                Console.WriteLine("No coordinats are specified in this sentence", e.Message); 
            }
            return coord;
        }
    }
}