using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

internal class Program
{
    private static void Main(string[] args)
    {
        // Inicialiar WebDriver
        using (var driver = new ChromeDriver())
        {
            // Captura um tempo para pesquisa
            Console.Write("O que deseja pesquisar? ");
            string strPesquisa = Console.ReadLine();
            if (strPesquisa == null || strPesquisa == "")
            {
                strPesquisa = "RPA";
            }

            // Navegando ao Site da Alura 
            driver.Navigate().GoToUrl("https://www.alura.com.br");

            // Procura pelo campo de busca para fazer input
            var searchBox = driver.FindElement(By.Id("header-barraBusca-form-campoBusca"));

            // Uma lógica só para validar que a pesquisa não veio vazia
            searchBox.SendKeys(strPesquisa);

            // Enter
            searchBox.SendKeys(Keys.Enter);

            // Um delay para esperar a página carregar
            Thread.Sleep(2000);

            // Validar que veio a lista de resultados
            if (driver.FindElements(By.Id("busca-resultados")).Any())
            {
                // Armazena os 10 primeiros itens que vieram do resultado
                var courseElements = driver.FindElements(By.CssSelector(".busca-resultado")).Take(10);

                // Lista para armazenar as info dos cursos
                List<(string, string, string, List<string>)> courseInfoList = new List<(string, string, string, List<string>)>();

                // Iteração através dos 10 primeiros registros
                foreach (var courseElement in courseElements)
                {
                    try
                    {
                        // Nome do Curso
                        var courseName = courseElement.FindElement(By.ClassName("busca-resultado-nome")).Text;
                        var courseDescription = courseElement.FindElement(By.ClassName("busca-resultado-descricao")).Text;

                        // Link do curso
                        var courseLink = courseElement.FindElement(By.CssSelector(".busca-resultado-link")).GetAttribute("href");

                        // Abre o curso em outra aba com JavaScript
                        ((IJavaScriptExecutor)driver).ExecuteScript("window.open(arguments[0], '_blank');", courseLink);

                        // Seleciona a aba nova
                        driver.SwitchTo().Window(driver.WindowHandles.Last());

                        // Delay para dar uma folga ao navegador
                        Thread.Sleep(2000);

                        // Valida se não é tela de login
                        if (driver.Title.Contains("Login | Alura"))
                        {
                            // Fecha a aba
                            driver.Close();

                            // Foca na aba principal
                            driver.SwitchTo().Window(driver.WindowHandles.First());

                            // Vai para o próximo registro
                            continue;
                        }

                        try
                        {
                            // Duração do Curso
                            var courseDurationElement = driver.FindElement(By.CssSelector(".courseInfo-card-wrapper-infos"));
                            var courseDuration = courseDurationElement.GetAttribute("innerText");

                            // Nome do(s) instrutor(es)
                            var instructorElement = driver.FindElement(By.CssSelector(".instructor-title--name"));
                            var instructorName = instructorElement.GetAttribute("innerText");

                            // Armazena as informações na lista
                            courseInfoList.Add((courseName, courseDescription, courseDuration, new List<string> { instructorName }));
                        }
                        catch (NoSuchElementException)
                        {
                            // Duração do Curso
                            var courseDurationElement = driver.FindElement(By.CssSelector(".formacao__info-conclusao .formacao__info-destaque"));
                            var courseDuration = courseDurationElement.GetAttribute("innerText");

                            // Nome(s) do(s) instrutor(es)
                            var instructorElements = driver.FindElements(By.CssSelector(".formacao-instrutor-nome"));
                            var instructorNames = instructorElements.Select(instructor => instructor.GetAttribute("innerText")).ToList();

                            // Armazena as informações na lista
                            courseInfoList.Add((courseName, courseDescription, courseDuration, instructorNames));
                        }

                        // Fecha a aba e volta para aba principal
                        driver.Close();
                        driver.SwitchTo().Window(driver.WindowHandles.First());
                    }
                    catch (WebDriverException ex)
                    {
                        // Handle WebDriver-related exceptions
                        Console.WriteLine("Error: WebDriver exception - " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        // Handle any other unexpected exceptions
                        Console.WriteLine("Error: An unexpected error occurred - " + ex.Message);
                    }
                }

                // Escreve as informações no arquivo de texto
                foreach (var courseInfo in courseInfoList)
                {
                    try
                    {
                        using (StreamWriter writer = new StreamWriter("database.txt", true)) // true para append
                        {
                            writer.WriteLine("Course: " + courseInfo.Item1);
                            writer.WriteLine("Description: " + courseInfo.Item2);
                            writer.WriteLine("Duration: " + courseInfo.Item3);
                            writer.WriteLine("Instructor(s): " + string.Join(", ", courseInfo.Item4));
                            writer.WriteLine();
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Error writing to file: " + ex.Message);
                    }
                }
            }
        }
    }
}
