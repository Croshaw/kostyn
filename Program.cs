using System.Globalization;

namespace InventorySimulation;

class Program
{
    static Random random = new Random();

    // Генерация времени между покупками (экспоненциальное распределение)
    static double GenerateTimeBetweenPurchases(double lambda)
    {
        return -Math.Log(1.0 - random.NextDouble()) / lambda;
    }

    // Генерация нормального распределения методом суммирования равномерных случайных величин
    static double GenerateNormalDistribution()
    {
        double sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += random.NextDouble();
        }
        return sum - 6;
    }
    
    static void Main(string[] args)
    {
        // Инициализация параметров
        int iterations = 1000; // Количество итераций
        
        double lambda = 10; // Интенсивность потока покупателей
        int safetyStock = 100; // Страховочный запас
        double mx = 5; // Среднее время поставки(mx)
        double sigma = 1; // Стандартное отклонение времени поставки(sigma)

        double a = 300; // Доход от продажи одной единицы(a)
        double b = 200; // Убыток за единицу дефицита(b)
        double c = 100; // Стоимость хранения одной единицы(c)

        // Списки для хранения результатов
        List<int> results = new List<int>(); // -1: дефицит, 1: профицит, 0: ни то, ни другое
        List<double> profits = new List<double>();

        // Основной цикл симуляции
        double dlambda = 1;
        int dsafety = 15;
        double dm = 0.5;
        List<List<double>> matrix = new List<List<double>>();

        for (double lambda0 = lambda - dlambda; lambda0 < lambda + dlambda; lambda0 += dlambda)
        {
            for (int safetyStock0 = safetyStock - dsafety; safetyStock0 < safetyStock + dsafety; safetyStock0 += dsafety)
            {
                for (double m0 = mx - dm; m0 < mx + dm; m0 += dm)
                {
                    List<double> array = new List<double> { lambda0, safetyStock0, m0 };

                    for (int n = 0; n < 3; n++)
                    {
                        for (int i = 0; i < iterations; i++)
                        {
                            int stock = 1000; // Изначальный запас товара
                            double t = 0; // Текущее время

                            // Симуляция покупок до достижения страховочного запаса
                            while (true)
                            {
                                double r = random.NextDouble();
                                double t0 = -Math.Log(1.0 - r) / lambda; // Экспоненциальное распределение
                                t += t0;

                                int purchasedItems = random.Next(1, 4); // Случайное число от 1 до 3
                                stock -= purchasedItems;

                                if (stock <= safetyStock)
                                {
                                    break;
                                }
                            }

                            double z = GenerateNormalDistribution();
                            double tr = mx + z * sigma; // Время до поставки
                            double tp = t + tr; // Время поставки

                            while (true)
                            {
                                double r = random.NextDouble();
                                double t0 = -Math.Log(1.0 - r) / lambda;
                                t += t0;

                                int purchasedItems = random.Next(1, 4);
                                stock -= purchasedItems;

                                if (t >= tp)
                                {
                                    break;
                                }
                            }

                            if (stock < 0)
                            {
                                results.Add(-1); // Дефицит
                                double pr = 1000 * a;
                                double def = Math.Abs(stock) * b;
                                double p = pr - def;
                                profits.Add(p);
                            }
                            else if (stock > 0)
                            {
                                results.Add(1); // Профицит
                                double x = 1000 - stock;
                                double pr = x * a;
                                double prof = stock * c;
                                double p = pr - prof;
                                profits.Add(p);
                            }
                            else
                            {
                                results.Add(0); // Ни профицита, ни дефицита
                                double p = 1000 * a;
                                profits.Add(p);
                            }
                        }

                        int surplusCount = 0, deficitCount = 0, neutralCount = 0;
                        foreach (int result in results)
                        {
                            if (result > 0) surplusCount++;
                            else if (result < 0) deficitCount++;
                            else neutralCount++;
                        }

                        double PS = 0;
                        foreach (double profit in profits)
                        {
                            PS += profit;
                        }
                        profits.Clear();

                        Console.WriteLine($"Случаев дефицита: {deficitCount}");
                        Console.WriteLine($"Случаев профицита: {surplusCount}");
                        Console.WriteLine($"Случаев без дефицита и профицита: {neutralCount}");
                        Console.WriteLine($"Прибыль: {PS/iterations}");
                        
                        array.Add(PS / 1000);
                    }

                    matrix.Add(array);
                }
            }
        }

        Console.WriteLine("x1\t\t\tx2\t\t\tx3\t\t\ty1\t\t\ty2\t\t\ty3");
        foreach (List<double> list in matrix)
        {
            foreach (double num in list)
            {
                Console.Write(num + "\t\t\t");
            }
            Console.WriteLine();
        }
        
        List<double> y_ = new List<double>(); // Список средних значений прибыли для каждой строки матрицы
        List<double> S_2 = new List<double>(); //Список значений дисперсий для каждой строки матрицы
        for (int i = 0; i < matrix.Count; i++)
        {
            double y__ = 0.0;
            var hz = matrix[i].Skip(3);
            y__ = hz.Sum();
            Console.WriteLine(string.Join(" + ", hz.Select(x => x.ToString(CultureInfo.InvariantCulture))) + $" = {y__}");
            // for (int j = 3; j < matrix[i].Count; j++)
            // {
            //     y__ += matrix[i][j];
            //     Console.Write(matrix[i][j] + " + ");
            // }
            // Console.WriteLine(" = " + y__);

            for (int j = 3; j < matrix[i].Count; j++)
            {
                S_2.Add(Math.Pow(matrix[i][j] - (y__ / 3), 2) / 2);
                y_.Add(y__ / 3);
            }
        }

        double sumS = S_2.Sum(); //Сумма всех значений дисперсий
        Console.WriteLine("S^2");
        Console.WriteLine(string.Join(", ", S_2.Select(x=>x.ToString(CultureInfo.InvariantCulture))));

        Console.WriteLine("y_");
        Console.WriteLine(string.Join(", ", y_.Select(x=>x.ToString(CultureInfo.InvariantCulture))));
        double maxS = S_2.Max(); // Максимальное значение дисперсии
        
        Console.WriteLine($"\nsum = {sumS} maxS = {maxS}");
        double G = maxS / sumS; // Критерий Кохрена
        Console.WriteLine($"\nG = {G}");
        
        // Расчет уравнения Фишера и адекватности модели
        var n1 = matrix.Count;  // количество экспериментов
        var m = 10;  // количество повторений каждого эксперимента
        var p1 = 3; // количество факторов

        double y_mean = y_.Average();
        double SS_total = y_.Sum(yi => Math.Pow(yi - y_mean, 2));
        double SS_residual = sumS;
        double SS_regression = SS_total - SS_residual;
        double df_regression = p1;
        double df_residual = n1 * m - p1 - 1;

        double MS_regression = SS_regression / df_regression;
        double MS_residual = SS_residual / df_residual;

        double F = MS_regression / MS_residual;
        
        // Табличное значение F для уровня значимости 0.05
        double alpha = 0.05;
        double fCritical = MathNet.Numerics.Distributions.FisherSnedecor.InvCDF(df_regression, df_residual, 1 - alpha);
        Console.WriteLine($"F-статистика: {F}");
        Console.WriteLine($"Критическое значение F: {fCritical}");

        // Проверка, отклоняем ли нулевую гипотезу
        if (F > fCritical)
            Console.WriteLine("Нулевая гипотеза отклоняется.");
        else
            Console.WriteLine("Нулевая гипотеза не отклоняется.");
    }
}