using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Hom3rFileReader
{
    enum fileOrigin_type { resources, URL } //file origin
    public enum fileType_type { ASCCI, binary, error }; //type to identify which type of files has been loaded

    fileOrigin_type fileOrigin; //to store the file origin
    fileType_type fileType; //to store the type of file

    //URL file manager                             
    StringReader textFile_URL_StringReader; //ASCII file manager
    byte[] bytesFile_URL_Array;
    int bytesFile_URL_Index;

    //Resources files
    StreamReader textFile_Resource_StreamReader; //resources text file manager
    FileStream binaryFile_Resource_FileStream; //resources binary file manager
    BinaryReader binaryFile_Resource_BinaryReader; //reads in binary
    
    public bool error = false;

    /////////////////
    //Constructors
    /////////////////
    public Hom3rFileReader() { }

    public Hom3rFileReader(string filePath)
    {
        string fileExtension = Right(filePath, 4);

        fileOrigin = fileOrigin_type.resources; //file is coming from file read from resources

        if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "stl")) //if stl file
        {
            StreamReader file = new StreamReader(filePath); //open the file        
            fileType = CheckFileType(file); //checks file type in a secondary function
            file.Close();//Close file
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "obj")) //if obj file
        {
            fileType = fileType_type.ASCCI; //only this format
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "3ds")) //if 3ds file
        {
            fileType = fileType_type.binary; //only this format
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "mtl")) //if obj file
        {
            fileType = fileType_type.ASCCI; //only this format
        }
        else
        {
            //ERROR 
            //FIXME resolve this case
        }

        //Open the file again, now using the correct way depending the type of file
        if (fileType == fileType_type.ASCCI)
        {
            textFile_Resource_StreamReader = new StreamReader(filePath);
        }
        else if (fileType == fileType_type.binary)
        {
            binaryFile_Resource_FileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            binaryFile_Resource_BinaryReader = new BinaryReader(binaryFile_Resource_FileStream);
        }
        else
        {
            error = true;
        }
    }

    public Hom3rFileReader(UnityWebRequest fileReadFromURL)
    {
        fileOrigin = fileOrigin_type.URL;       //Save file origin

        //Check file extension and save fileType
        string fileExtension = Right(fileReadFromURL.url, 4);
                
        if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "stl")) //if stl file
        {
            fileType = CheckFileType(fileReadFromURL); //checks file type in a secondary function
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "obj")) //if obj file
        {
            fileType = fileType_type.ASCCI; //only this format
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "3ds")) //if 3ds file
        {
            fileType = fileType_type.binary; //only this format
        }
        else if ((fileExtension[0] == '.') && (Right(fileExtension, 3) == "mtl")) //if material file
        {
            fileType = fileType_type.ASCCI; //only this format
        }
        else
        {
            //ERROR 
            //FIXME resolve this case
        }

        //Prepare the string to be read
        if (fileType == fileType_type.ASCCI)
        {
            textFile_URL_StringReader = new StringReader(fileReadFromURL.downloadHandler.text);
        }
        else if (fileType == fileType_type.binary)
        {
            bytesFile_URL_Array = fileReadFromURL.downloadHandler.data;
            bytesFile_URL_Index = 0;
        }
        else
        {
            error = true;
        }
    }

    /////////////////
    // Get Methods
    /////////////////
    public fileType_type GetFileType() { return fileType; }

    //ASCII file
    public string GetLine()
    {
        string temp = "";

        if (fileType == fileType_type.ASCCI)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = textFile_URL_StringReader.ReadLine();

                if (temp != null)
                {
                    temp = temp.Trim();
                }
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = textFile_Resource_StreamReader.ReadLine();

                if(temp != null)
                {
                    temp = temp.Trim();
                }
            }
        }

        return temp;
    }

    //Binary File    
    public string GetString(int size)
    {
        string temp = "";

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = System.Text.Encoding.UTF8.GetString(SubArray(bytesFile_URL_Array, bytesFile_URL_Index, size));
                bytesFile_URL_Index += size;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = System.Text.Encoding.UTF8.GetString(binaryFile_Resource_BinaryReader.ReadBytes(size));
            }
        }

        return temp;
    }

    public UInt32 GetUInt()
    {
        UInt32 temp = 0;

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = BitConverter.ToUInt32(SubArray(bytesFile_URL_Array, bytesFile_URL_Index, 4), 0);
                bytesFile_URL_Index = bytesFile_URL_Index + 4;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = binaryFile_Resource_BinaryReader.ReadUInt32();
            }
        }

        return temp;
    }

    public ushort GetUShort()
    {
        ushort temp = 0;

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = BitConverter.ToUInt16(SubArray(bytesFile_URL_Array, bytesFile_URL_Index, 2), 0);
                bytesFile_URL_Index = bytesFile_URL_Index + 2;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = binaryFile_Resource_BinaryReader.ReadUInt16();
            }
        }

        return temp;
    }

    public float GetFloat()
    {
        float temp = 0.0f;

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = BitConverter.ToSingle(SubArray(bytesFile_URL_Array, bytesFile_URL_Index, 4), 0);
                bytesFile_URL_Index = bytesFile_URL_Index + 4;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = binaryFile_Resource_BinaryReader.ReadSingle();
            }
        }

        return temp;
    }
    
    public char GetChar()
    {
        char temp =  ' ';

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = (char)System.Text.Encoding.UTF8.GetString(SubArray(bytesFile_URL_Array, bytesFile_URL_Index, 1))[0];
                bytesFile_URL_Index += 1;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = (char)System.Text.Encoding.UTF8.GetString(binaryFile_Resource_BinaryReader.ReadBytes(1))[0];
            }
        }

        return temp;
    }

    public byte GetByte()
    {
        byte temp = 0;

        if (fileType == fileType_type.binary)
        {
            if (fileOrigin == fileOrigin_type.URL)
            {
                temp = SubArray(bytesFile_URL_Array, bytesFile_URL_Index, 1)[0];
                bytesFile_URL_Index += 1;
            }
            else if (fileOrigin == fileOrigin_type.resources)
            {
                temp = binaryFile_Resource_BinaryReader.ReadByte();
            }
        }

        return temp;
    }

    public void Seek(uint size)
    {
        if (fileOrigin == fileOrigin_type.resources)
        {
            binaryFile_Resource_BinaryReader.BaseStream.Seek(size, SeekOrigin.Current);
        }
        else if (fileOrigin == fileOrigin_type.URL)
        {
            bytesFile_URL_Index += (int)size;
        }
        
    }

    public long Position()
    {
        long temp = 0;

        if (fileOrigin == fileOrigin_type.resources)
        {
            temp = binaryFile_Resource_BinaryReader.BaseStream.Position;
        }
        else if (fileOrigin == fileOrigin_type.URL)
        {
            temp = bytesFile_URL_Index;
        }

        return temp;
    }

    public long Length()
    {
        long temp = 0;
        
        if (fileOrigin == fileOrigin_type.resources)
        {
            temp = binaryFile_Resource_BinaryReader.BaseStream.Length;
        }
        else if (fileOrigin == fileOrigin_type.URL)
        {
            temp = bytesFile_URL_Array.Length;
        }

        return temp;
        
    }

    /////////////////
    // Others
    /////////////////
    public void Close()
    {
        if (fileOrigin == fileOrigin_type.resources)
        {
            if (fileType == fileType_type.ASCCI)
            {
                textFile_Resource_StreamReader.Close();
            }
            else if (fileType == fileType_type.binary)
            {
                binaryFile_Resource_FileStream.Close();
            }
        }
    }

    /////////////////
    // Private
    /////////////////
    /// <summary>Check if the file is binary or ASCII</summary>
    /// <param name="file"></param>
    /// <returns></returns>
    fileType_type CheckFileType(StreamReader file)
    {
        string first = file.ReadLine().Trim(); //reads the first line of the file and remove the spaces

        if (first != null) //if don't fail
        {
            //checks if the file is ASCII     
            if (first.StartsWith("solid"))
            {
                //if the line start with "solid" return true, is correct ASCII STL file  
                return fileType_type.ASCCI;
            }

            //checks if the file is binary
            for (int i = 0; i < 80; i++) //check the first 80 bytes
            {
                //reads 1 byte and checks
                if (file.Read() == 0x0) 
                {
                    //is binary
                    return fileType_type.binary;
                }
            }
        }

        return fileType_type.error;
    }

    /// <summary>Check if the file is binary or ASCII</summary>
    /// <param name="str"></param>
    /// <returns></returns>
    fileType_type CheckFileType(UnityWebRequest fileReadFromURL)
    {        
        StringReader sr = new StringReader(fileReadFromURL.downloadHandler.text); //Get the StringReader
                
        string first = sr.ReadLine(); //Read first line and check

        if (first != null) //if don't fail
        {
            //checks if the file is ASCII
            if (first.StartsWith("solid")) //if the line start with "solid" return that it is a correct ASCII STL file
            {
                return fileType_type.ASCCI;
            }

            //checks if the file is binary
            byte[] tempByte = fileReadFromURL.downloadHandler.data;

            for (int i = 0; i < 80; i++) //check the first 80 bytes
            {
                //reads 1 byte and checks
                if (tempByte[i] == 0x0) //is binary
                {                   
                    return fileType_type.binary;
                } 
            }
        }

        return fileType_type.error;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    byte[] SubArray(byte[] data, int index, int length)
    {
        byte[] result = new byte[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

    public static string Right(string param, int length)
    {
        string result = param.Substring(param.Length - length, length);
        return result;
    }
}