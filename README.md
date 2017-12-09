# ClassMakerWithJson
리플렉션을 이용해 Json 데이터를 파싱할 수 있는 데이터 클래스를 자동으로 생성한다. 

# Json example

## sample1
<pre><code>
{ "x":0.0, "y":0.0, "z":0.0 }
</code></pre>

### sample1 result class

<pre><code>
public SampleClass
{
  public double x;
  public double y;
  public double z;
}
</code></pre>

## sample2
<pre><code>
{
   "strtype":"string",
   "arrSample":
   [
      {  
         "body":
         {  
            "inttype":1,
            "doubletype":0.0,
            "booltype":true,
            "longtype":9999999999
         }
      }
   ]
}
</code></pre>

### sample2 result classes
<pre><code>

public class SampleClass
{
  public string strtype;
  public SampleClass_arrSample[] arrSample;
}

public SampleClass_arrSample
{
  SampleClass_arrSample_body body;
}

public SampleClass_arrSample_body
{
  public int inttype;
  public double doubletype;
  public bool booltype;
  public long longtype;  
}

</pre></code>

# Used Plugin
-LitJson (https://lbv.github.io/litjson/)




