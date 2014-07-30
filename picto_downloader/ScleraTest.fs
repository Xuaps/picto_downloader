namespace picto_downloader
open System
open NUnit.Framework
open Picto.Sclera
open FSharp.Data
open HtmlAgilityPack.FSharp
open System.IO
open System.Text

[<TestFixture>]
type ScleraTest() = 

    let getCategoriesPage =
        "http://www.sclera.be/en/picto/cat_overview"
        |>Http.RequestString
    
    let getCategoryPage cat pag =
        "http://www.sclera.be/en/picto/" + cat + "/p/" + pag
        |>Http.RequestString

    let forEach  action collection = 
        Array.ForEach collection
    


    let getImage url=
        try
            let stream="http://www.sclera.be/" + url
                        |>Http.RequestStream
                        |>(fun r->r.ResponseStream)
            let mStream = new MemoryStream()
            stream.CopyTo mStream
            Convert.ToBase64String(mStream.ToArray())
        with _ ->
            "Error"

    [<Test>]
    member x.GetCategories() =
        Assert.AreEqual([|[|"Abstract";"/cat/1"|];[|"Animals";"/cat/2"|]|], Pictos.getCategories "<html><head></head><body><ul class='clearfix'><li><a href='/cat/1'>Abstract</a></li><li><a href='/cat/2'>Animals</a></li></ul></body></html>")
    
    [<Test>]
    member x.GetPictos() =
        let pictos = Pictos.getPictos "<html><head></head><body><div class='picto-by-name-list'><h3 class='picto-name'>seasons</h3><div class='group'><a href='/en/picto/detail/21353' class='picto-tTitle><span><img src='/resources/pictos/seizoenen%20t.png'></span></a></div><h3 class='picto-name'>sunday</h3><div class='group'><a href='/en/picto/detail/19083' class='picto-thumb'><span><img src='/resources/pictos/pellenberg/zondag%20t.png'></span></a><a href='/en/picto/detail/19047' class='picto-thumb'><span><img src='/resources/pictos/kleur/zondag blauw%20t.png'></span></a></div></div></body></html>"
        Assert.AreEqual("sunday", pictos.[1].Tittle)
        Assert.AreEqual([|"/resources/pictos/pellenberg/zondag.png";"/resources/pictos/kleur/zondag blauw.png"|], pictos.[1].Images)
    
    [<Test>]
    member x.GetCategoryPageNumbers() =
        Assert.AreEqual(["1";"2";"3";"4"], Pictos.getCategoryPages "<html><head></head><body><div class='paging'><p><a href='/en/picto/cat/1/p/1'>&lt; Previous</a> |<span class='current'>1</span> | <a href='/en/picto/cat/1/p/2'>2</a> | <a href='/en/picto/cat/1/p/3'>3</a> | <a href='/en/picto/cat/1/p/4'>4</a> | <a href='/en/picto/cat/1/p/2'>Next &gt;</a></p></div>  </body></html>")

//    [<Test>]
//    member x.GetCategoryWithPictos() = 
//        Assert.AreEqual([{Tittle="Abstract";Pictos=[|{Tittle="sunday";Images=[|"/resources/pictos/pellenberg/zondag.png";"/resources/pictos/kleur/zondag blauw.png"|]}|]}],
//            Pictos.getCategory [|"Abstract";"/cat/1"|])
            
    [<Test>]
    member x.SavePictos() = 
        let categories=getCategoriesPage
                    |>Pictos.getCategories
                    |>Array.map (fun cat-> Pictos.getCategoryPages(getCategoryPage cat.[1] "1")
                                            |>Seq.map (fun n->Pictos.getPictos (getCategoryPage cat.[1] n))
                                            |>Seq.concat
                                            |>Seq.toArray
                                            |>(fun c->{Tittle=cat.[0];Pictos=c}))
        
        let outFile = new StreamWriter("pictos.csv")
        for i=0 to (Array.length categories)-1 do
            for j=0 to (Array.length categories.[i].Pictos)-1 do
                for k=0 to (Array.length categories.[i].Pictos.[j].Images)-1 do
                    let image = getImage categories.[i].Pictos.[j].Images.[k]
                    outFile.WriteLine(sprintf "%s,%s,%s,%s" categories.[i].Tittle categories.[i].Pictos.[j].Tittle categories.[i].Pictos.[j].Images.[k] image)