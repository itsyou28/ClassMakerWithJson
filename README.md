# ClassMakerWithJson
리플렉션을 이용해 입력한 json 데이터와 매칭되는 클래스를 정의한 dll을 생성한다.

# 실행방법
1. Tools -> ClassMaker
2. 텍스트 입력단에 json 데이터를 입력
3. Create 버튼 클릭
4. Assets/Plugins/[SampleName].dll 파일 생성 확인
5. 솔루션 참조 내역에서 AutoClass/SampleName이 확인되면 성공

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




