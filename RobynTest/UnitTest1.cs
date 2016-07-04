using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobynHandMadeSoap;
using System.Collections.Generic;

namespace RobynTest
{
    [TestClass]
    public class UploadFileTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            byte a = new byte();
            a = 1;
            Dictionary<string, int[]> filesToSave = new Dictionary<string, int[]>(2);
            filesToSave.Add("Main",new int[] {100,100});
            RobynHandMadeSoap.Models.UploadedFile testFile = new RobynHandMadeSoap.Models.UploadedFile()
            {
                FileName = "My TEST File",
                ContentType = "Image/JPG",
                FileExt = "jpg",
                FileSize = 10,
                Contents = new byte[10]{a,a,a,a,a,a,a,a,a,a},
                FileRelPath = "/main/",
                FilesToSave = filesToSave
            };

            string x = testFile.filePathCombine("C:\\dir1", "/images/", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\images\\tom.jpg", x, "FIRST filePathCombine Does not work correctly");

            
            x = testFile.filePathCombine("C:\\dir1", "/images/bv/", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\images\\bv\\tom.jpg", x, "SECOND filePathCombine Does not work correctly");

            x = testFile.filePathCombine("C:\\dir1", "/images/bvc", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\images\\bvc\\tom.jpg", x, "THIRD filePathCombine Does not work correctly");

            x = testFile.filePathCombine("C:\\dir1", "/", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\tom.jpg", x, "FOURTH filePathCombine Does not work correctly");

            x = testFile.filePathCombine("C:\\dir1", "images", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\images\\tom.jpg", x, "FIFTH filePathCombine Does not work correctly");


            x = testFile.filePathCombine("C:\\dir1\\", "images/bv", "\\", "tom.jpg");
            Assert.AreEqual("C:\\dir1\\images\\bv\\tom.jpg", x, "SIXTH filePathCombine Does not work correctly");
        }
    }
}
