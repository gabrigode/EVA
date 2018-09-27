/* REGRAS DE PROGRAMAÇÃO
 * -----------Regras de declaração de variável-------------------------------------------

	NOMEVAR_SUFIXO

Nomes Sufixos:

_som -> variável que armazena efeito sonoro
_tmr -> variável contador do timer
_id  -> variável que armazena um valor id utilizado para localização de ponto do jogo
_rdm -> variável que armazena valor randômico
_fala-> variável que armazena resultado da fala
_sct -> variável que armazena valor de outra variável já atribuida
_bin -> variável que armazena valor binário (0 ou 1)
_cont-> variável que realiza contagem

--------------Regras de declaração de objeto---------------------------------------------

	NOMEOBJ_SUFIXO

Nomes Sufixos:

_timer -> Timer
_mp    -> Windows Midia Player
_img   -> PictureBox

-----------------------------------------------------------------------------------------
*/


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Speech.Recognition; 
using System.Threading;
using System.Media; 

namespace WindowsFormsApplication5
{
    public partial class Form1 : Form
    {
        SoundPlayer explosao_som = new SoundPlayer(@"arquivos\audios\explosao.wav"); //explosão final de game over
        SoundPlayer desvio_som = new SoundPlayer(@"arquivos\audios\desvio.wav"); //desvio de nave
        SoundPlayer esquerda_som = new SoundPlayer(@"arquivos\audios\esquerda.wav"); //tiro vindo da esquerda
        SoundPlayer direita_som = new SoundPlayer(@"arquivos\audios\direita.wav"); //tiro vindo da direita
        SoundPlayer impactoesq_som = new SoundPlayer(@"arquivos\audios\impdir.wav"); //impacto na esquerda
        SoundPlayer impactodir_som = new SoundPlayer(@"arquivos\audios\imesq.wav");// impacto na direita
        int vida_cont; //do jogador
        int tempo_tmr; //intervalo de reação
        int resul_tiro_rdm;//variável que armazena numero aleatório para ataque
        int verifica_sirene_bin;//variável de segurança, usada na verificação da sirene
        int result_tiro_sct;// variável de segurança de dado, sempre recebe valor de 'n'
        int subgame_id;//armazena id correspondente ao minigame, utilizada no reconhecimento
        int tempo_padrao_tmr; //tempo padrao
        int tela_id;
        int tela_cont;
        string fala;
        int ganha_cont;
        int inicio_id;
        int timer_som_cont;
        int tiro_bin;
        int tiro_bin2;
        /*INDICE ID MINIGAME

        111 -> morto
        -2  -> inicio
        -1  -> configuração
        0   -> falas aleatórias
        1   -> desvio de tiros
        7   -> ATIRAR
        222 -> recomeça ganha

         */

        private static SpeechRecognitionEngine engine; //Reconhecimento de Fala
        public Form1()
        {
            InitializeComponent();
        }

//Reconhecimento de Fala (Carregamento)
        private void LoadSpeech()
        {
                engine = new SpeechRecognitionEngine(); //caso haja erro nesta linha o problema é com os pacotes, verificar também a versão do sistema: 32 ou 64 bits
                engine.SetInputToDefaultAudioDevice(); //caso haja erro nesta linha significa que o programa não conseguiu localizar um microfone para ser utilizado no reconhecimento

                string[] words = {"iniciar", "fechar", "esquerda", "direita", "atirar", "sim", "não" };//gramática, novas palavras a serem reconhecidas devem ser introduzidas aqui

                engine.LoadGrammar(new Grammar(new GrammarBuilder(new Choices(words))));//carrega gramática
                engine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(rec);
                engine.RecognizeAsync(RecognizeMode.Multiple);
        }



//----------------------------------------------Timers--------------------------------------------------
 //Timer Padrão
        private void timer_padrao_Tick(object sender, EventArgs e)
        {

            tempo_padrao_tmr++;
            if (inicio_id == 77)
            {
                if (tempo_padrao_tmr == 30)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    inicio_id = 0;
                    inicioataque();
                }

            }
            if (inicio_id == 42)
            {
                if (tempo_padrao_tmr == 10)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    pictureBox1.Visible = true;
                    inicio_id = 0;
                    subgame_id = 4;                    
                }
            }
            else if (inicio_id == 1)
            {
                if (tempo_padrao_tmr == 14)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    inicio_id = 0;
                    ataque();
                }
            }
            if (vida_cont == 6)
            {
                if (tempo_padrao_tmr == 6)
                {
                    timer_padrao.Stop();
                    tempo_padrao_tmr = 0;
                    começo();
                }
            }
            else
            {
                if (subgame_id == 222)
                {
                    if (tempo_padrao_tmr == 14)
                    {
                        tempo_padrao_tmr = 0;
                        timer_padrao.Stop();
                        começo();
                    }
                }
                if (subgame_id == 111)
                {
                    if (tempo_padrao_tmr == 51)
                    {
                        timer_padrao.Stop();
                        tempo_padrao_tmr = 0;
                        config();
                    }
                }
                if (subgame_id == 7)
                {
                    if (tempo_padrao_tmr == 4)
                    {
                        tempo_padrao_tmr = 0;
                        timer_padrao.Stop();
                        ganha_cont = 0;
                        ataque();
                    }
                }
            }
        }

//Timer Som
        private void timer_som_Tick(object sender, EventArgs e)
        {
            timer_som_cont++;
            if (timer_som_cont == 2)
            {
                timer_som.Stop();
                timer_som_cont = 0;
                sirene();
                verifica();
            }
        }

//Timer Imagem
        private void timer_imagem_Tick(object sender, EventArgs e)
        {
            tela_cont++;
            switch (tela_id) {
                case 1:
                    if (tela_cont == 4)
                    {
                        pictureBox2.Image = Image.FromFile("C:\\arquivos\\gif\\abertura.png");
                        axWindowsMediaPlayer2.URL = @"arquivos\audios\iniciarfechar.wav";
                    }
                    if (tela_cont == 8)
                    {
                        pictureBox1.Visible = true;
                        timer_imagem.Stop();
                        tela_cont = 0;
                    }
                break;
                case 2:
                    if (tela_cont >= 33)
                    {
                        pictureBox2.Image = Image.FromFile(@"arquivos\gif\fone.png");
                        if (tela_cont == 51)
                        {
                            pictureBox2.Image = Image.FromFile(@"arquivos\gif\fone direita.gif");
                            tela_cont = 0;
                            pictureBox1.Visible = true;
                            timer_imagem.Stop();
                        }
                    }
                    break;
            }
        }

 //Timer Tiros
        private void timer1_Tick(object sender, EventArgs e)
        {
            tempo_tmr++;//adiciona 1 a cada 1 segundo a variavel tempo
                if (tempo_tmr == 3)//tempo de reação (do disparo até o impacto)
                {
                    if (result_tiro_sct == 0)
                    {
                        impactoesq_som.Play();
                        vida_cont++;
                        timer_som.Start();
                    }
                    else
                    {
                        impactodir_som.Play();
                        vida_cont++;
                        timer_som.Start();
                    }
                }

          }
            

//------------------------------------------------------------------------------------------------------


//Load Form
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSpeech(); //inicia o reconhecimento
            começo();
        }


//Configurações iniciais
        private void começo()
        {
            timer_som_cont = 0;
            inicio_id = 0;
            ganha_cont = 0;
            fala = "";
            tempo_padrao_tmr = 0;
            subgame_id = 111;
            tempo_tmr = 0;
            result_tiro_sct = 3;
            verifica_sirene_bin = 0;
            vida_cont = 0;
            axWindowsMediaPlayer1.URL = @"arquivos\audios\fundo.wav"; //som de fundo
            axWindowsMediaPlayer1.settings.playCount = 1000; //quantidade de repetição do audio
            axWindowsMediaPlayer1.settings.volume = 10; //volume do audio
            axWindowsMediaPlayer2.URL = @"arquivos\audios\abertura.wav";
            axWindowsMediaPlayer2.settings.volume = 200;
            pictureBox2.Image = Image.FromFile("C:\\arquivos\\gif\\abertura.gif");
            tela_id = 1;
            timer_imagem.Start();
            subgame_id = -2;
            tiro_bin = 0;
            tiro_bin2 = 0;
        }


//Chama ação do Reconhecimento de fala
        private void rec(object s, SpeechRecognizedEventArgs e)
        {
            fala = (e.Result.Text);
            comandosvoz();
        }

//Comandos Voz
private void comandosvoz()
        {
            switch (subgame_id)//limitação de palavras da gramática baseado no id de cada caso
            {
                case 4:
                    switch (fala)
                    {
                        case "sim":
                            fala = "";
                            pictureBox1.Visible = false;
                            subgame_id = 111;
                            drive();
                            break;
                        case "não":
                            fala = "";
                            axWindowsMediaPlayer2.URL = @"arquivos\audios\Nao_negue.wav";
                            break;
                    }
                    break;
                case 7:
                    switch (fala)
                    {
                        case "atirar":
                            fala = "";
                            timer_padrao.Stop();
                            tempo_padrao_tmr = 0;
                            fim();

                            break;
                    }
                    break;
                case -2:
                    switch (fala)
                    {
                        case "iniciar":
                            fala = "";
                            pictureBox1.Visible = false;
                            inicio();
                            subgame_id = 111;
                            break;
                        case "fechar":
                            Close();
                            break;

                    }
                    break;
                case -1:
                    switch (fala)
                    {
                        case "direita":
                            pictureBox1.Visible = false;
                            parabens();
                            fala = "";
                            subgame_id = 111;
                            break;
                        case "esquerda":
                            pictureBox1.Visible = false;
                            axWindowsMediaPlayer2.URL = @"arquivos\audios\Inverta.mp3";
                            fala = "";
                            break;
                    }
                    break;
                case 1:
                    if (vida_cont < 5)
                    {
                        //tempo_tmr = 0;//seta a variável de tempo
                        //timer1.Stop();//para o timer
                        if (timer_som_cont == 0)
                        {
                            switch (fala) //variável que possui o resultado obtido no reconhecimento
                            {
                                case "esquerda":
                                    if (result_tiro_sct == 0)
                                    {
                                        fala = "";
                                        impactoesq_som.Play();
                                        vida_cont++;
                                        timer1.Stop();
                                        timer_som.Start();
                                        // sirene();
                                        //verifica();
                                    }
                                    else
                                    {

                                        fala = "";
                                        ganha_cont++;
                                        desvio_som.Play();
                                        pictureBox2.Image = Image.FromFile(@"arquivos\gif\tiro_direita.gif");
                                        timer1.Stop();
                                        timer_som.Start();
                                        //verifica();
                                    }
                                    break;
                                case "direita":
                                    if (result_tiro_sct == 1)
                                    {

                                        fala = "";
                                        impactodir_som.Play();
                                        vida_cont++;
                                        timer1.Stop();
                                        timer_som.Start();
                                        //sirene();
                                        //verifica();
                                    }
                                    else
                                    {

                                        fala = "";
                                        ganha_cont++;
                                        desvio_som.Play();
                                        pictureBox2.Image = Image.FromFile(@"arquivos\gif\tiro_esquerda.gif");
                                        timer1.Stop();
                                        timer_som.Start();
                                        //verifica();
                                    }
                                    break;

                            }
                        }
                    }
                    break;
            }
        }


//Inicio Jogo (Apresentação)
        private void inicio()
        {
            axWindowsMediaPlayer2.URL = @"arquivos\audios\Carregamento_ola.wav";
            axWindowsMediaPlayer2.settings.volume = 2000;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\igor.gif");
            timer_padrao.Start();
            tela_id = 2;
            timer_imagem.Start();
        }


//Configurações ID
        private void config()
        {
            subgame_id = -1; //id
        }


        private void parabens()
        {
            subgame_id = 111;
            inicio_id = 42;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\igor.gif");
            axWindowsMediaPlayer2.URL = @"arquivos\audios\Parabens.wav";
            timer_padrao.Start();
        }
        

//hyperdrive
        private void drive()
        {
            inicio_id = 77;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\hyperspace.gif");
            // aqui animação hyperdrive
            axWindowsMediaPlayer2.URL = @"arquivos\audios\hyperdrive.mp3";
            timer_padrao.Start();
        }
//Inicio do Ataque
        private void inicioataque()
        {
            inicio_id = 1;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\sirene.gif");
            axWindowsMediaPlayer2.URL = @"arquivos\audios\preataque.wav";
            timer_padrao.Start();
        }


//Ataque
        private void ataque()//ataque da nave inimiga
        {
                result_tiro_sct = 3;
                resul_tiro_rdm = 3;
                timer1.Stop();
                tempo_tmr = 0;//seta a variável de tempo
                subgame_id = 1;
                //LoadSpeech(); //inicia o reconhecimento
                Random randNum = new Random();
                resul_tiro_rdm = randNum.Next(2); //gera numero aleatório
            if (tiro_bin2 == 2)
            {
                resul_tiro_rdm = 1;
                tiro_bin2 = 0;
            }
            else if (tiro_bin == 2)
            {
                resul_tiro_rdm = 0;
                tiro_bin = 0;
            }
                //n = 0; //linha para teste (tira a aleatoriedade, sempre será esquerda)

                //Thread.Sleep(1500);//intervalo para carregar audio
                if (resul_tiro_rdm == 0)
                {
                    tiro_bin2++;
                    esquerda_som.Play();
                    pictureBox2.Image = Image.FromFile(@"arquivos\gif\impacto_esquerda.gif");
                    timer1.Start(); //inicia o timer
                    result_tiro_sct = resul_tiro_rdm;
                }

                if (resul_tiro_rdm == 1)
                {
                    tiro_bin++;
                    direita_som.Play();
                    pictureBox2.Image = Image.FromFile(@"arquivos\gif\impacto_direita.gif");
                    timer1.Start(); //inicia o timer
                    result_tiro_sct = resul_tiro_rdm;
                }
        }


//Verifica Sirene
        private void sirene()//ativa sons de sirene depois de três impactos
        {
            if (vida_cont == 3 && verifica_sirene_bin != 1)
            {
                verifica_sirene_bin = 1;//impede reprodução repitida
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\sirene.gif");
                axWindowsMediaPlayer2.URL = @"arquivos\audios\sirene.mp4";
                axWindowsMediaPlayer2.settings.playCount = 2000;
            }
        }


//Verifica Game Over 
        private void verifica()//verifica se jogador perdeu, caso contrário chama ataque() novamente
        {
            if (vida_cont < 5)//para alterar quantidade de vidas do jogador modifique esta linha
            {
                if (ganha_cont >= 5)
                {
                    subgame_id = 7;
                    timer1.Stop();
                    tempo_padrao_tmr = 0;
                    axWindowsMediaPlayer2.settings.playCount = 0;
                    axWindowsMediaPlayer2.URL = @"arquivos\audios\atirar.wav";
                    axWindowsMediaPlayer2.settings.volume = 3000;
                    timer_padrao.Start();
                }
                else
                {
                    ataque();
                }
            }
            else //game over
            {
                subgame_id = 111;
                vida_cont = 6;
                axWindowsMediaPlayer2.Ctlcontrols.stop();
                axWindowsMediaPlayer1.Ctlcontrols.stop();
                explosao_som.Play();
                axWindowsMediaPlayer2.settings.playCount = 0;
                axWindowsMediaPlayer2.URL = @"arquivos\audios\fimdejogo.mp3";
                pictureBox2.Image = Image.FromFile(@"arquivos\gif\gameover.jpg");
                axWindowsMediaPlayer2.settings.volume = 2000;
                timer_padrao.Start();
                timer1.Stop();
            }
        }


//Fim de Jogo  
        private void fim()
        {
            subgame_id = 222;
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            axWindowsMediaPlayer2.settings.playCount = 0;
            pictureBox2.Image = Image.FromFile(@"arquivos\gif\preto.png");
            axWindowsMediaPlayer2.URL = @"arquivos\audios\fim.wav";
            timer_padrao.Start();
        }

//Comando setas
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        { 
            if (e.KeyCode == Keys.Left) {
                fala = "esquerda";
            }
            if (e.KeyCode == Keys.Right)
            {
                fala = "direita";
            }
            if (e.KeyCode == Keys.Up)
            {
                fala = "atirar";
            }
            if (e.KeyCode == Keys.Enter)
            {
                fala = "iniciar";
            }
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
            if (e.KeyCode == Keys.Y)
            {
                fala = "sim";
            }
            if (e.KeyCode == Keys.N)
            {
                fala = "não";
            }
            comandosvoz();
        }


    }
}
