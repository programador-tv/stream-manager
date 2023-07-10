using System.Diagnostics;

public class ProcessManager
{

    public string Id;
    private Process process;

    public ProcessManager(string id)
    {
        this.Id = id;
        var baseDirectory = Directory.GetCurrentDirectory() + "/Assets/Lives/";
    

        string hlsSegmentPath = Path.Combine(baseDirectory, $"{id}-%d.ts");
        string hlsPlaylistPath = Path.Combine(baseDirectory, $"{id}.m3u8");

        process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                //aqui é onde o diabo vive
                    $"-re -i pipe:0"
                    + $" -c:v libx264 -r 30 -g 30 -preset ultrafast -tune zerolatency -b:v 1M -pix_fmt yuv420p -c:a aac -b:a 128k "
                    + $" -f hls -hls_playlist_type event -hls_time 7 -hls_list_size 0 -hls_segment_filename {hlsSegmentPath} {hlsPlaylistPath} ",
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
    }

    public void Run()
    {
        process.Start();
    }

    public void WriteToStandardInput(byte[] input)
    {
        try
        {
            MemoryStream stream = new MemoryStream(input);
            stream.CopyTo(process.StandardInput.BaseStream);
            stream.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + "--- deu ruim na hora de escrever ;ç");
        }
    }

    public async Task StopAsync()
    {
        try
        {
            if (!process.HasExited)
            {
                process.StandardInput.BaseStream.Close();
                await process.WaitForExitAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + " ----- deu ruim na hora de parar o processo");
        }
        finally
        {
            process.Dispose();
        }
    }

    public async Task ReadOutputAsync()
    {
        string line;
        while ((line = await process.StandardOutput.ReadLineAsync()) != null)
        {
            Console.WriteLine("---SAMPLE: " + line);
        }
    }

    public async Task ReadErrorAsync()
    {
        string line;
        while ((line = await process.StandardError.ReadLineAsync()) != null)
        {
            Console.WriteLine("---ERROR:  " + line);
        }
    }
}
