#include "hx711.h"
#include "Delay.h"

float average;

void hx711_init(void)
{
	HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_SET);
  delay_ms(10);
  HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_RESET);
  delay_ms(10);
}	

int32_t getHX711(void)
{
  uint32_t data = 0;
  uint32_t startTime = HAL_GetTick();
  while(HAL_GPIO_ReadPin(DT_PORT, DT_PIN) == GPIO_PIN_SET)
  {
    if(HAL_GetTick() - startTime > 100)
      return 0;
  }
	
  for(int8_t len=0; len < 24 ; len++)
  {
    HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_SET);
    delay_us(1);
    data = data << 1;
    HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_RESET);
    delay_us(1);
    if(HAL_GPIO_ReadPin(DT_PORT, DT_PIN) == GPIO_PIN_SET)
      data ++;
  }
  data = data ^ 0x800000;
  HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_SET);
  delay_us(1);
  HAL_GPIO_WritePin(SCK_PORT, SCK_PIN, GPIO_PIN_RESET);
  delay_us(1);
  return data;
}

float get_weight_avg(uint16_t samples)
{
  int32_t  total = 0;
  float average;
  float span,zero;
  for(uint16_t i=0 ; i<samples ; i++)
  {
      total += getHX711();
  }
	
  average = (float)(((int)((total / samples))));
	
	//zero = average - offset;
  //span = know_weight / (average-offset);
	
  //milligram = (int)(average-offset)*6;
  return average;
}
int get_zero()
{
	float tare;
	average = get_weight_avg(20);
	tare = average;
	return tare;
}	

float get_span(float know_weight_max,float zero2)
{
	float span;
	
	average = get_weight_avg(20);
	
	span = (float)((average-zero2-8820580)/know_weight_max );
	return span;
}	

float get_weight_calib(float zero_calib,float span_calib)
{
	float gram;
	average = get_weight_avg(20);
	gram = (((float)(average-zero_calib-8820580)*span_calib))/1000;
	return gram;
}	
