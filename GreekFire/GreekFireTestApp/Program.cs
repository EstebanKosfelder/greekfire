// See https://aka.ms/new-console-template for more information

using CGAL;

using System.Numerics;




//var rnd = new Random();
//var list = Enumerable.Range(0, 1000).Select(e => new HeapItemBase<double>(rnd.NextDouble())).ToList();
//var queue = new  HeapBase<double,HeapItemBase<double>>(list );
//queue.heapify();

//double anterior = 0.0;
//while (!queue.empty())
//{
//    var item = queue.pop();
//    if (anterior > item.priority)
//    {

//    }
//    anterior = item.priority;
//    Console.WriteLine($"Next event at t={item.priority}");
//}





GreekFireBuilder builder = new GreekFireBuilder();


var polys = GreekFireBuilder.CargarListaVector2("F:\\trabajos\\g_test.json");




foreach (var poly in polys)
{
    
    builder.EnterContour(poly.Select(p=>new Point2(p.X,p.Y) ));
}



builder.Initialize();
builder.init_event_list();

WavefrontPropagator wp = new WavefrontPropagator(builder);



while (!wp.propagation_complete() && !builder.EventQueue.empty())
{
    //var item =(EventQueueItem) ( builder.EventQueue.pop();
    //builder.handle_event(item.priority);




    wp.advance_step();

    }



    Console.WriteLine("fin");
